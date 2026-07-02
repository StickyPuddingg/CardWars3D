// ponytail: v2 format - adds a deduplicated string table (covers both field names and values)
// plus a per-file schema, so repeated keys/values (measured: 99.4% of value occurrences in
// db_Decks.json are repeats of one of only 806 unique strings) are stored once and referenced
// by index instead of by raw bytes. Source JSON already stores every field as a quoted string
// (verified: zero unquoted numbers/bools/nulls across the whole Blueprints folder), so the
// table holds strings only - matches the runtime shape BlueprintBinaryReader_dbl reconstructs.
//
// Format (little-endian, matches System.IO.BinaryReader regardless of host endianness):
//   magic "BDB2" (4 bytes)
//   int32 stringTableCount, then per string: int32 byteLen + utf8 bytes
//   int16 schemaFieldCount, then per field: int32 stringTableIndex (the field's name)
//   int32 recordCount, then per record: int16 presentFieldCount,
//     then per present field: int16 schemaFieldIndex + int32 stringTableIndex (value)
//
// Usage: node tools/json_to_bin.js <file1.json> [file2.json ...]
//        node tools/json_to_bin.js --dir <folder>   (converts every *.json in folder)

const fs = require('fs');
const path = require('path');

function convertFile(jsonPath) {
	const raw = fs.readFileSync(jsonPath, 'utf8');
	const data = JSON.parse(raw);
	if (!Array.isArray(data)) {
		console.error(`SKIP (not an array): ${jsonPath}`);
		return;
	}

	// Build dedup string table (field names + values share one table) and schema (field order).
	const stringIndex = new Map(); // string -> index
	const strings = [];
	function intern(s) {
		let idx = stringIndex.get(s);
		if (idx === undefined) {
			idx = strings.length;
			strings.push(s);
			stringIndex.set(s, idx);
		}
		return idx;
	}

	const schemaIndex = new Map(); // fieldName -> schema slot
	const schemaFieldNameStringIdx = []; // schema slot -> string table index

	const encodedRecords = [];
	for (const record of data) {
		const keys = Object.keys(record);
		const fields = [];
		for (const key of keys) {
			let slot = schemaIndex.get(key);
			if (slot === undefined) {
				slot = schemaFieldNameStringIdx.length;
				schemaFieldNameStringIdx.push(intern(key));
				schemaIndex.set(key, slot);
			}
			const value = record[key];
			const valueStr = value === null || value === undefined ? '' : String(value);
			fields.push([slot, intern(valueStr)]);
		}
		encodedRecords.push(fields);
	}

	// Serialize.
	const buffers = [Buffer.from('BDB2', 'ascii')];

	const strCountBuf = Buffer.alloc(4);
	strCountBuf.writeInt32LE(strings.length, 0);
	buffers.push(strCountBuf);
	for (const s of strings) {
		const bytes = Buffer.from(s, 'utf8');
		const lenBuf = Buffer.alloc(4);
		lenBuf.writeInt32LE(bytes.length, 0);
		buffers.push(lenBuf, bytes);
	}

	const schemaCountBuf = Buffer.alloc(2);
	schemaCountBuf.writeInt16LE(schemaFieldNameStringIdx.length, 0);
	buffers.push(schemaCountBuf);
	for (const strIdx of schemaFieldNameStringIdx) {
		const buf = Buffer.alloc(4);
		buf.writeInt32LE(strIdx, 0);
		buffers.push(buf);
	}

	const recCountBuf = Buffer.alloc(4);
	recCountBuf.writeInt32LE(encodedRecords.length, 0);
	buffers.push(recCountBuf);
	for (const fields of encodedRecords) {
		const fieldCountBuf = Buffer.alloc(2);
		fieldCountBuf.writeInt16LE(fields.length, 0);
		buffers.push(fieldCountBuf);
		for (const [slot, valIdx] of fields) {
			const slotBuf = Buffer.alloc(2);
			slotBuf.writeInt16LE(slot, 0);
			const valBuf = Buffer.alloc(4);
			valBuf.writeInt32LE(valIdx, 0);
			buffers.push(slotBuf, valBuf);
		}
	}

	const outPath = jsonPath.replace(/\.json$/i, '.bin');
	const outBuffer = Buffer.concat(buffers);
	fs.writeFileSync(outPath, outBuffer);

	const jsonSize = Buffer.byteLength(raw);
	const binSize = outBuffer.length;
	console.log(`${path.basename(jsonPath)}: ${data.length} records, ${strings.length} unique strings, ${jsonSize}B -> ${binSize}B (${(100 * binSize / jsonSize).toFixed(0)}%)`);
}

function main() {
	const args = process.argv.slice(2);
	if (args.length === 0) {
		console.error('Usage: node json_to_bin.js <file.json...> | --dir <folder>');
		process.exit(1);
	}

	let files = [];
	if (args[0] === '--dir') {
		const dir = args[1];
		files = fs.readdirSync(dir)
			.filter(f => f.toLowerCase().endsWith('.json'))
			.map(f => path.join(dir, f));
	} else {
		files = args;
	}

	for (const file of files) {
		try {
			convertFile(file);
		} catch (e) {
			console.error(`FAILED: ${file}: ${e.message}`);
		}
	}
}

main();
