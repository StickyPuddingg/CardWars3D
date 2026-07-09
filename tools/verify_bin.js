// Decodes a .bin file (BDB2 format) the same way BlueprintBinaryReader_dbl.cs does, and
// compares the result against the original .json to catch encoding bugs before Unity testing.
const fs = require('fs');

function decodeBin(buf) {
	let offset = 0;
	function readInt32() { const v = buf.readInt32LE(offset); offset += 4; return v; }
	function readInt16() { const v = buf.readInt16LE(offset); offset += 2; return v; }
	function readString(len) { const v = buf.toString('utf8', offset, offset + len); offset += len; return v; }

	const magic = buf.toString('ascii', offset, offset + 4); offset += 4;
	if (magic !== 'BDB2') throw new Error('bad magic: ' + magic);

	const stringCount = readInt32();
	const strings = [];
	for (let i = 0; i < stringCount; i++) {
		const len = readInt32();
		strings.push(readString(len));
	}

	const schemaCount = readInt16();
	const schemaNames = [];
	for (let i = 0; i < schemaCount; i++) {
		schemaNames.push(strings[readInt32()]);
	}

	const recordCount = readInt32();
	const records = [];
	for (let i = 0; i < recordCount; i++) {
		const fieldCount = readInt16();
		const rec = {};
		for (let f = 0; f < fieldCount; f++) {
			const slot = readInt16();
			const valIdx = readInt32();
			rec[schemaNames[slot]] = strings[valIdx];
		}
		records.push(rec);
	}

	if (offset !== buf.length) throw new Error(`trailing bytes: consumed ${offset} of ${buf.length}`);
	return records;
}

function verify(jsonPath, binPath) {
	const original = JSON.parse(fs.readFileSync(jsonPath, 'utf8'));
	const decoded = decodeBin(fs.readFileSync(binPath));

	if (original.length !== decoded.length) {
		return `FAIL record count: json=${original.length} bin=${decoded.length}`;
	}
	for (let i = 0; i < original.length; i++) {
		const o = original[i];
		const d = decoded[i];
		const okeys = Object.keys(o);
		const dkeys = Object.keys(d);
		if (okeys.length !== dkeys.length) {
			return `FAIL record ${i} field count: json=${okeys.length} bin=${dkeys.length}`;
		}
		for (const k of okeys) {
			const ov = o[k] === null || o[k] === undefined ? '' : String(o[k]);
			if (d[k] !== ov) {
				return `FAIL record ${i} field "${k}": json="${ov}" bin="${d[k]}"`;
			}
		}
	}
	return 'OK';
}

const args = process.argv.slice(2);
if (args[0] === '--dir') {
	const dir = args[1];
	const files = fs.readdirSync(dir).filter(f => f.toLowerCase().endsWith('.json'));
	let failures = 0;
	for (const f of files) {
		const jsonPath = `${dir}/${f}`;
		const binPath = jsonPath.replace(/\.json$/i, '.bin');
		if (!fs.existsSync(binPath)) { console.log(`${f}: NO BIN FILE`); failures++; continue; }
		const result = verify(jsonPath, binPath);
		if (result !== 'OK') { console.log(`${f}: ${result}`); failures++; }
	}
	console.log(`\n${files.length - failures}/${files.length} passed`);
} else {
	console.log(verify(args[0], args[1] || args[0].replace(/\.json$/i, '.bin')));
}
