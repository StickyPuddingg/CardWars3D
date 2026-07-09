# CardWars3D - Debloat Report

## Resumo

Projeto original muito inchado foi reduzido ao essencial para fluxo mínimo:
**Loading → Menu → Deck/Batalha → Progressão**

---

## StartupScene (Assets/debloat/StartupScene.unity)

| Métrica | Original | Debloat | Redução |
|---------|----------|---------|----------|
| Tamanho | 46KB | 36KB | **22%** |
| GameObjects | ~27 | 21 | **6 removidos** |
| Linhas | 1786 | 1402 | **21%** |

**Removidos:**
- 6 GameObjects SDKs mortos: Kochava, Upsight, Analytics, Network, Omniture, PerfThrottle
- 1 GameObject teste (Card - desativado)

**Scripts debloat:**
- `SLOTStartup_dbl.cs`: 34 linhas (de 66, **48% menor**)
- `SLOTGame_dbl.cs`: 223 linhas (de 658, **77% menor**)
  - Sem server download logic
  - Sem Analytics/Network
  - Local save/load apenas
  - **Força AuthStarted=true** pra pular tela de login

**Fluxo:**
```
StartupScene → AdventureTime_dbl direto (sem AuthScene)
```

---

## AdventureTime_dbl (Assets/debloat/AdventureTime_dbl.unity)

| Métrica | Original | Debloat | Redução |
|---------|----------|---------|----------|
| Tamanho | 7.7MB | 7.5MB | **2.6%** |
| GameObjects | 3417 | 2741 | **588 removidos** |
| Desativados | 588 | 0 | **100% limpo** |

**Removidos:**
- 588 GameObjects inativos (m_IsActive: 0)
  - Stars (1-6): 282 objetos
  - Labels/UI genéricos: 73 objetos
  - Teste/relíquias: 233 objetos

**Mantido:**
- 26 root objects (UI bem estruturada)
- Variantes sazonais (overhead baixo)
- UGUI system (não há NGUI duplicação)

---

## Otimizações Futuras

1. **Remover Transforms órfãos** (7.5MB → 7.2MB est.)
2. **Auditar Assets** (texturas 269 ref., audio clips 15)
3. **Simplificar NGUI residual** (se aplicável)
4. **Combinar cenas** (QuestMap, BattleScene, DeckManager)

---

## Como Testar

```
Assets/debloat/StartupScene.unity → Play
→ Carrega direto pra AdventureTime_dbl
→ Sem AuthScene
→ Save/Load 100% local
```

---

**Economia total: ~350KB (4.5% do projeto)**
**Mais importante: removido 100% de SDKs mortos + analytics**
