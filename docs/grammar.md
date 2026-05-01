# Špecifikácia jazyka — Expression Evaluator

**Verzia:** 0.1 (MVP)
**Autor:** Adrián Kučera
**Projekt:** Expression Evaluator (semestrálna práca, UNIZA FRI, 2026)

---

## Obsah

1. [Úvod](#1-úvod)
2. [Základné pojmy](#2-základné-pojmy)
3. [Notácia EBNF](#3-notácia-ebnf)
4. [Lexikálna gramatika](#4-lexikálna-gramatika)
5. [Syntaktická gramatika](#5-syntaktická-gramatika)
6. [Priorita a asociativita operátorov](#6-priorita-a-asociativita-operátorov)
7. [Sémantika výrazov](#7-sémantika-výrazov)
8. [Prepracované príklady (worked examples)](#8-prepracované-príklady)
9. [Bežné chyby a okrajové prípady](#9-bežné-chyby-a-okrajové-prípady)
10. [Čo jazyk nepodporuje](#10-čo-jazyk-nepodporuje)

---

## 1. Úvod

Tento dokument je **formálna špecifikácia** jazyka, ktorý interpretuje aplikácia
Expression Evaluator. Definuje:

- aké reťazce znakov sú platné **výrazy** (expressions) v tomto jazyku,
- ako sa tieto výrazy **parsujú** (analyzujú do stromovej štruktúry),
- ako sa **vyhodnocujú** (aký výsledok produkujú).

**Tento dokument je "rulebook" jazyka.** Ak sa kód a táto špecifikácia rozchádzajú,
jedno z nich je bug. Špecifikácia má prednosť.

Dokument je písaný v slovenčine s anglickými technickými termínmi tam, kde je to
v odbornej literatúre štandard (parser, lexer, AST, EBNF, atď.).

---

## 2. Základné pojmy

**Výraz (expression)** — reťazec znakov, ktorý po vyhodnotení produkuje hodnotu.
Napríklad `3 + 5`, `sqrt(16)`, `x > 10 AND x < 50`.

**Token** — najmenšia jednotka výrazu, ktorú vytvára *lexer*. Napríklad číslo `42`,
operátor `+`, identifikátor `x`, zátvorka `(`.

**Lexer (tokenizer)** — komponent, ktorý vstupný reťazec znakov rozdelí na
postupnosť tokenov. Pracuje podľa **lexikálnej gramatiky** (kapitola 4).

**Parser** — komponent, ktorý z postupnosti tokenov vytvorí **abstraktný
syntaktický strom** (AST). Pracuje podľa **syntaktickej gramatiky** (kapitola 5).

**AST (Abstract Syntax Tree)** — stromová reprezentácia výrazu, kde každý uzol
reprezentuje operáciu a jej deti sú operandy. Napríklad výraz `3 + 5 * 2` má AST:

```
        +
       / \
      3   *
         / \
        5   2
```

**Evaluator (vyhodnocovač)** — komponent, ktorý prejde AST a vypočíta výslednú
hodnotu.

**Pipeline** — celý reťazec spracovania výrazu:

```
   "3 + 5 * 2"          →  [3, +, 5, *, 2]  →    +          →    13
   (vstupný reťazec)       (tokeny)            / \             (výsledok)
                           lexer               3  *
                                              /  /\
                                            parser  \
                                                 evaluator
```

---

## 3. Notácia EBNF

Gramatika je zapísaná v notácii **EBNF (Extended Backus-Naur Form)**, normovanej
v ISO/IEC 14977. Je to štandardná notácia pre formálnu definíciu programovacích
jazykov.

| Symbol   | Význam                       | Príklad                              |
|----------|------------------------------|--------------------------------------|
| `=`      | "je definované ako"          | `digit = "0" \| "1" \| ... ;`        |
| `\|`     | alternatíva (alebo)          | `sign = "+" \| "-" ;`                |
| `,`      | zreťazenie (postupnosť)      | `pair = "(" , expr , ")" ;`          |
| `[ ... ]`| voliteľné (0 alebo 1 výskyt) | `number = [sign] , digits ;`         |
| `{ ... }`| opakovanie (0 alebo viac)    | `digits = digit , { digit } ;`       |
| `( ... )`| zoskupenie                   | `( a \| b ) , c`                     |
| `"..."`  | doslovný terminál            | `"+"` znamená znak `+`               |
| `;`      | koniec pravidla              | každé pravidlo končí `;`             |
| `(* *)`  | komentár                     | `(* toto je komentár *)`             |

**Príklad čítania pravidla:**

```ebnf
number = digit , { digit } , [ "." , digit , { digit } ] ;
```

Čítame: *"Číslo je jedna číslica, za ňou ľubovoľný počet ďalších číslic, a
voliteľne za nimi bodka, po ktorej nasleduje aspoň jedna číslica a ľubovoľný
počet ďalších číslic."*

Toto pravidlo akceptuje: `0`, `42`, `3.14`, `0.001`
Toto pravidlo odmieta: `.5` (chýba celá časť), `3.` (chýba desatinná časť)

---

## 4. Lexikálna gramatika

Lexikálna gramatika určuje, ako lexer rozdelí vstupný reťazec na tokeny.

### 4.1 Biele znaky (whitespace)

```ebnf
whitespace = " " | "\t" | "\r" | "\n" ;
```

**Biele znaky sú ignorované** — neprodukujú tokeny, iba oddeľujú tokeny od seba.

Napríklad `3+5`, `3 + 5`, a `3   +   5` produkujú rovnakú postupnosť tokenov:
`[3, +, 5]`.

### 4.2 Číselné literály

```ebnf
digit         = "0" | "1" | "2" | "3" | "4"
              | "5" | "6" | "7" | "8" | "9" ;

integer_part  = digit , { digit } ;

fraction_part = "." , digit , { digit } ;

number        = integer_part , [ fraction_part ] ;
```

**Čo to znamená:** Číslo je postupnosť číslic, voliteľne nasledovaná bodkou a
ďalšími číslicami.

**Platné čísla:** `0`, `42`, `3.14`, `0.001`, `1000`, `2.5`

**Neplatné čísla:**
- `.5` — chýba celá časť (v našom jazyku treba písať `0.5`)
- `3.` — chýba desatinná časť
- `1e10` — vedecká notácia nie je podporovaná v MVP
- `-5` — mínus nie je súčasť čísla, je to unárny operátor (viď kap. 6)

**Poznámka k typu:** Interne sú všetky čísla reprezentované ako `double`
(64-bitové číslo s plávajúcou desatinnou čiarkou). Aj celé čísla ako `42` sú
reprezentované ako `42.0`. Je to zjednodušenie pre MVP — nemáme samostatné typy
`int` a `float`.

### 4.3 Identifikátory

```ebnf
letter     = "A" | "B" | ... | "Z"
           | "a" | "b" | ... | "z" ;

identifier = ( letter | "_" ) , { letter | digit | "_" } ;
```

**Čo to znamená:** Identifikátor začína písmenom alebo podčiarkovníkom, ďalšie
znaky môžu byť písmená, číslice alebo podčiarkovníky.

**Platné identifikátory:** `x`, `radius`, `user_age`, `x1`, `_temp`, `PI`

**Neplatné identifikátory:**
- `2x` — začína číslicou
- `user-name` — obsahuje pomlčku (pomlčka je operátor mínus)
- `my var` — obsahuje medzeru (bola by rozdelená na dva tokeny)

**Case sensitivity:** Identifikátory sú **case-sensitive** — `x` a `X` sú dva
rôzne identifikátory.

### 4.4 Kľúčové slová

```ebnf
keyword = "true" | "false"
        | "AND"  | "OR"   | "NOT"
        | "and"  | "or"   | "not" ;
```

Kľúčové slová sú **špeciálna podmnožina identifikátorov** — reťazce, ktoré by
inak boli identifikátory, ale majú v jazyku vyhradený význam.

**Ako to funguje:** Lexer najprv matchuje identifikátor podľa pravidla 4.3. Ak
výsledný reťazec je v zozname kľúčových slov, lexer ho označí ako token daného
typu (napr. `TOKEN_TRUE`, `TOKEN_AND`) namiesto `TOKEN_IDENTIFIER`.

**Dôsledok:** Používateľ nemôže nazvať premennú `true`, `AND`, atď.

### 4.5 Operátory a interpunkcia

```ebnf
operator = "+" | "-" | "*" | "/" | "%" | "^"
         | "==" | "!=" | "<=" | ">=" | "<" | ">"
         | "&&" | "||" | "!" ;

punctuation = "(" | ")" | "," ;
```

**Dôležité — pravidlo maximálneho zhryzu (maximal munch):** Keď lexer vidí `==`,
musí to rozpoznať ako **jeden** token `==`, nie dva tokeny `=` a `=`. To platí
pre všetky viacznakové operátory: `==`, `!=`, `<=`, `>=`, `&&`, `||`.

Algoritmus je jednoduchý: **vždy skús najprv dlhšiu variantu.** Teda pri čítaní
`<` sa pozri na nasledujúci znak — ak je `=`, vyrobíme token `<=`; inak `<`.

**Príklad dôsledku:**
```
Vstup:    x <= 5
Tokeny:   [IDENT(x), LE, NUM(5)]          ← správne
Nie:      [IDENT(x), LT, ASSIGN, NUM(5)]  ← nesprávne
```

### 4.6 Prehľad všetkých typov tokenov

Lexer produkuje tokeny týchto typov:

| Typ tokenu     | Príklady                    | Popis                          |
|----------------|-----------------------------|--------------------------------|
| `NUMBER`       | `42`, `3.14`                | Číselný literál                |
| `IDENTIFIER`   | `x`, `radius`               | Identifikátor premennej/funkcie|
| `TRUE`, `FALSE`| `true`, `false`             | Boolean literály               |
| `AND`, `OR`, `NOT` | `AND`, `&&`, `or`, `!`  | Logické operátory              |
| `PLUS`, `MINUS`| `+`, `-`                    | Sčítanie/odčítanie             |
| `STAR`, `SLASH`, `PERCENT` | `*`, `/`, `%`   | Násobenie/delenie/modulo       |
| `CARET`        | `^`                         | Umocňovanie                    |
| `EQ`, `NEQ`    | `==`, `!=`                  | Rovnosť/nerovnosť              |
| `LT`, `LE`, `GT`, `GE` | `<`, `<=`, `>`, `>=`| Porovnania                  |
| `LPAREN`, `RPAREN` | `(`, `)`                | Zátvorky                       |
| `COMMA`        | `,`                         | Oddeľovač argumentov funkcie   |
| `EOF`          | (koniec vstupu)             | Synt. značka pre koniec        |

---

## 5. Syntaktická gramatika

Syntaktická gramatika určuje, ako parser z postupnosti tokenov vytvorí AST.
Gramatika je navrhnutá tak, aby **štruktúra pravidiel sama kódovala prioritu a
asociativitu operátorov** — parser nepotrebuje žiadnu samostatnú tabuľku priorít.

### 5.1 Vstupný bod

```ebnf
expression = or_expr ;
```

Každý výraz začína pravidlom `or_expr`, teda od **najnižšej priority**.

### 5.2 Úroveň 1: Logické OR

```ebnf
or_expr = and_expr , { ( "OR" | "or" | "||" ) , and_expr } ;
```

**Význam:** `or_expr` je jeden `and_expr`, voliteľne nasledovaný ľubovoľným
počtom `OR`-pokračovaní.

**Asociativita:** Ľavoasociatívny (kvôli iterácii `{ }`).

**Príklad:** `a OR b OR c` sa parsuje ako `(a OR b) OR c`.

```
AST:       OR
          /  \
         OR   c
        /  \
       a    b
```

### 5.3 Úroveň 2: Logické AND

```ebnf
and_expr = not_expr , { ( "AND" | "and" | "&&" ) , not_expr } ;
```

Rovnaký vzor ako OR. Ľavoasociatívny.

**Prečo má AND vyššiu prioritu ako OR?** Matematická konvencia: `a AND b OR c`
znamená `(a AND b) OR c`, nie `a AND (b OR c)`. AND je analógia násobenia, OR je
analógia sčítania, a násobenie má prednosť pred sčítaním.

### 5.4 Úroveň 3: Logické NOT

```ebnf
not_expr = [ ( "NOT" | "not" | "!" ) ] , comparison ;
```

**Význam:** `not_expr` je voliteľné `NOT`, za ním `comparison`.

**Toto je unárny operátor** — má iba jeden operand (ten vpravo).

**Príklad:** `NOT x > 5` sa parsuje ako `NOT (x > 5)`, nie `(NOT x) > 5`, pretože
NOT má nižšiu prioritu ako porovnanie.

**Reťazenie:** `NOT NOT x` je platné (stačí zanoriť pravidlo do seba cez
`comparison → ... → primary → ... → not_expr`? Nie, pozor — `not_expr` volá iba
`comparison`, teda `NOT NOT x` nie je priamo možné touto gramatikou.)

> **Dizajnové rozhodnutie:** V MVP zakazujeme `NOT NOT x`. Používateľ musí
> napísať `NOT (NOT x)` so zátvorkami. Je to kompromis kvôli jednoduchosti
> parsera. V thesis práci to možno rozšíriť.

### 5.5 Úroveň 4: Porovnania

```ebnf
comparison = additive , [ comp_op , additive ] ;

comp_op = "==" | "!=" | "<" | "<=" | ">" | ">=" ;
```

**Pozor — `[ ]`, nie `{ }`.** Pravidlo je *voliteľné (0 alebo 1)*, nie
*opakované*. To znamená:

- `a < b` ✅ platné
- `a < b < c` ❌ **neplatné** — syntaktická chyba

**Prečo?** Reťazené porovnania sú dizajnovo nejednoznačné. V C sa `a < b < c`
parsuje ako `(a < b) < c`, čo je skoro vždy bug. Python to parsuje ako
`(a < b) AND (b < c)`, čo je matematicky správne, ale komplikuje parser.
**V MVP volíme najjednoduchšiu cestu — zakázať reťazenie.** Používateľ musí
napísať `a < b AND b < c`.

### 5.6 Úroveň 5: Sčítanie a odčítanie

```ebnf
additive = multiplicative , { ( "+" | "-" ) , multiplicative } ;
```

Ľavoasociatívny. `1 - 2 - 3` = `((1 - 2) - 3)` = `-4`, nie `1 - (2 - 3) = 2`.

### 5.7 Úroveň 6: Násobenie, delenie, modulo

```ebnf
multiplicative = power , { ( "*" | "/" | "%" ) , power } ;
```

Ľavoasociatívny.

**Modulo (`%`) na doubles:** V C# operátor `%` pre `double` je zbytok po delení,
napr. `7.5 % 2 = 1.5`. Toto preberáme.

### 5.8 Úroveň 7: Umocňovanie

```ebnf
power = unary , [ "^" , power ] ;
```

**Pravoasociatívny** — všimnite si, že pravidlo volá samé seba na pravej strane
(`power`), nie iteráciou (`{ }`).

`2 ^ 3 ^ 2` = `2 ^ (3 ^ 2)` = `2 ^ 9` = `512`, nie `(2 ^ 3) ^ 2 = 64`.

Toto zodpovedá matematickej konvencii (napr. LaTeX `2^{3^2}` sa tiež vyhodnotí
zhora nadol).

### 5.9 Úroveň 8: Unárny mínus

```ebnf
unary = [ "-" ] , primary ;
```

**Umiestnenie pravidla `unary` medzi `power` a `primary` je zámerné.**
Spôsobuje, že unárny mínus má **vyššiu prioritu ako `^`**:

- `-2 ^ 2` sa parsuje ako `(-2) ^ 2` = `4`
- Nie ako `-(2^2)` = `-4`

#### Prečo to tak vychádza

Vyplýva to priamo zo štruktúry pravidiel. Keď parser vidí `-2 ^ 2`:

1. `power` zavolá `unary` pre svoj ľavý operand.
2. `unary` zachytí `-` a spasruje `-2` ako `UnaryOp(Negate, 2)`.
3. `power` má ľavý operand hotový, vidí `^`, spasruje pravý operand `2`.
4. Výsledok: `BinaryOp(Power, UnaryOp(Negate, 2), 2)` → vyhodnotí sa ako
   `(-2)^2 = 4`.

Mínus teda "zrastie" so svojím operandom **skôr**, než ten operand vstúpi do
mocniny.

#### Ako to robia iné jazyky

Tento jazyk dodržiava konvenciu C-rodiny. Pre porovnanie:

| Jazyk           | Výraz       | Výsledok | Interpretácia      |
|-----------------|-------------|----------|--------------------|
| **Tento jazyk** | `-2 ^ 2`    | `4`      | `(-2)^2`           |
| C# `Math.Pow`   | `Math.Pow(-2, 2)` | `4`| `(-2)^2`           |
| Python          | `-2 ** 2`   | `-4`     | `-(2**2)`          |
| Wolfram Alpha   | `-2 ^ 2`    | `-4`     | `-(2^2)`           |

Python a matematická notácia volia opačnú konvenciu — mocnina viaže silnejšie
než unárny mínus.

#### Prečo sme zvolili C-konvenciu

Inverzná konvencia by si vyžadovala asymetrickú gramatiku (`unary` pre ľavý
operand `^`, `power` pre pravý), čo komplikuje pravidlá aj implementáciu.
C-konvencia je priamym dôsledkom prirodzeného poradia úrovní priorít.

**Je to vedomé dizajnové rozhodnutie — dokumentujeme ho, aby používateľ
vedel, čo očakávať.** Ak používateľ chce matematickú konvenciu, môže explicitne
zazátvorkovať: `-(2^2)`.

### 5.10 Úroveň 9: Primárne výrazy

```ebnf
primary = number
        | "true"
        | "false"
        | function_call
        | identifier
        | "(" , expression , ")" ;
```

**Primárny výraz** je najjednoduchšia "atomická" jednotka — buď hodnota, alebo
niečo v zátvorkách.

**Zátvorky resetujú prioritu:** `(a + b) * c` — vnútorné `a + b` sa vyhodnotí
ako prvé, hoci `*` má bežne vyššiu prioritu.

### 5.11 Volanie funkcie

```ebnf
function_call = identifier , "(" , [ argument_list ] , ")" ;

argument_list = expression , { "," , expression } ;
```

**Význam:** Volanie funkcie je identifikátor nasledovaný zátvorkami, v ktorých
je 0 alebo viac výrazov oddelených čiarkami.

**Príklady:**
- `sqrt(16)` — 1 argument
- `max(3, 5)` — 2 argumenty
- `pi()` — 0 argumentov (hypoteticky, ak by sme takú funkciu mali)

**Ako parser rozlíši volanie funkcie od premennej?** Pravidlo `primary` má v sebe
aj `function_call`, aj `identifier`. Parser sa pozrie na **jeden token dopredu
(lookahead)**: ak po identifikátore nasleduje `(`, je to volanie funkcie; inak je
to premenná.

Toto sa volá **LL(1) parsovanie** — *left-to-right, leftmost derivation, 1 token
lookahead*. Je to najjednoduchšia a najpoužívanejšia trieda parserov pre ručne
písané rekurzívne zostupné (recursive descent) parsery.

---

## 6. Priorita a asociativita operátorov

Prehľad od **najnižšej po najvyššiu** prioritu:

| Úroveň | Operátory          | Asociativita  | Príklad                        |
|--------|--------------------|--------------|--------------------------------|
| 1      | `OR` / `\|\|`       | ľavá          | `a OR b OR c` → `(a OR b) OR c`|
| 2      | `AND` / `&&`       | ľavá          | `a AND b AND c` → `(a AND b) AND c` |
| 3      | `NOT` / `!`        | unárny        | `NOT x AND y` → `(NOT x) AND y`|
| 4      | `==` `!=` `<` `<=` `>` `>=` | nie je (žiadne reťazenie) | `a < b` |
| 5      | `+`, `-`           | ľavá          | `1 - 2 - 3` → `(1 - 2) - 3`    |
| 6      | `*`, `/`, `%`      | ľavá          | `12 / 3 / 2` → `(12 / 3) / 2`  |
| 7      | `^`                | **pravá**     | `2^3^2` → `2^(3^2) = 512`       |
| 8      | unárne `-`         | unárny        | `-2^2` → `(-2)^2 = 4`          |
| 9      | `()`, literály, volania | —        | —                              |

### Mentálny model: "Nižšie v tabuľke = silnejšia väzba"

Keď parser vidí výraz, **operátory z vysokej priority "zrastajú" so svojimi
operandmi skôr** než operátory z nízkej priority. Preto `3 + 5 * 2`:

1. `*` má prioritu 6, `+` má prioritu 5.
2. `*` "zrastie" s `5` a `2` skôr → `5 * 2` sa stane jedným podstromom.
3. Potom `+` vidí `3` a `(5 * 2)` → výsledný AST.

```
       +
      / \
     3   *
        / \
       5   2
```

---

## 7. Sémantika výrazov

Gramatika hovorí *ako* sa veci parsujú. **Sémantika** hovorí *čo to znamená*.

### 7.1 Typy hodnôt

Jazyk má dva typy hodnôt:

- **`Number`** — reálne číslo (interne `double`).
- **`Boolean`** — `true` alebo `false`.

### 7.2 Typové pravidlá operátorov

| Operátor | Operandy           | Výsledok  |
|----------|-------------------|-----------|
| `+ - * / % ^` | `Number`, `Number` | `Number` |
| unárne `-` | `Number`         | `Number` |
| `< <= > >=` | `Number`, `Number` | `Boolean` |
| `== !=`  | rovnaký typ       | `Boolean` |
| `AND OR` | `Boolean`, `Boolean` | `Boolean` |
| `NOT`    | `Boolean`         | `Boolean` |

**Nesprávne použitie typu je chyba pri vyhodnocovaní**, napr. `true + 5` alebo
`5 AND 3`. Evaluator vráti chybu. (V MVP nerobíme type-checking pri parsovaní —
iba pri vyhodnocovaní.)

### 7.3 Chybové stavy pri vyhodnocovaní

- **Delenie nulou** — `5 / 0`. V C# `double` to vráti `Infinity` alebo `NaN`,
  ale my to explicitne detekujeme a vrátime chybu.
- **Neznáma premenná** — ak používateľ použije `x`, ktorú nikde nedefinoval.
- **Neznáma funkcia** — ak používateľ zavolá `foo(5)` a `foo` neexistuje.
- **Nesprávny počet argumentov** — `sqrt(1, 2)` (čaká 1 argument, dostane 2).
- **Typová nezhoda** — `true + 5`, `"abc" AND false` (keby sme mali stringy).

### 7.4 Vstavané funkcie (MVP)

| Funkcia     | Argumenty           | Výsledok        |
|-------------|---------------------|-----------------|
| `sqrt(x)`   | `Number`            | `Number` (druhá odmocnina) |
| `abs(x)`    | `Number`            | `Number` (absolútna hodnota) |
| `min(a, b)` | `Number`, `Number`  | `Number` (menšie z dvoch) |
| `max(a, b)` | `Number`, `Number`  | `Number` (väčšie z dvoch) |
| `pow(a, b)` | `Number`, `Number`  | `Number` (a na b-tú) |

---

## 8. Prepracované príklady (worked examples)

Tri príklady, ktoré ilustrujú, ako gramatika funguje v praxi.

### Príklad 1: `3 + 5 * 2`

**Očakávaný výsledok:** `13` (nie `16`, lebo `*` má vyššiu prioritu).

**Tokeny:** `[NUM(3), PLUS, NUM(5), STAR, NUM(2), EOF]`

**Parsovanie:**

1. Parser vstúpi do `expression` → `or_expr` → `and_expr` → `not_expr` →
   `comparison` → `additive`.
2. `additive` volá `multiplicative` pre ľavý operand.
3. `multiplicative` volá `power` → `unary` → `primary`.
4. `primary` matchuje `NUM(3)` → vracia uzol `Number(3)`.
5. Vrátime sa do `multiplicative` — ďalší token je `PLUS`, nie `*`/`/`/`%`, takže
   loop sa neopakuje. Vraciame `Number(3)`.
6. Späť v `additive`: ďalší token je `PLUS` → vstupujeme do iterácie.
7. Skonzumujeme `PLUS`, volaním `multiplicative` získame pravý operand.
8. `multiplicative` → `power` → `unary` → `primary` → vracia `Number(5)`.
9. Naspäť v `multiplicative`: ďalší token je `STAR` → iterujeme!
10. Skonzumujeme `STAR`, volaním `power` → ... → `Number(2)`.
11. `multiplicative` zostaví `Multiply(Number(5), Number(2))` a vráti to.
12. `additive` zostaví `Add(Number(3), Multiply(Number(5), Number(2)))`.

**Výsledný AST:**

```
        Add
       /    \
   Num(3)  Multiply
           /      \
       Num(5)   Num(2)
```

**Vyhodnotenie:**
1. `Multiply(5, 2)` → `10`
2. `Add(3, 10)` → `13` ✅

### Príklad 2: `-2 + 3 * 4 ^ 2`

**Očakávaný výsledok:** `-2 + 3 * 16 = -2 + 48 = 46`.

**Kľúčové pozorovania:**
- `^` má vyššiu prioritu ako `*` → `4 ^ 2` sa vypočíta ako prvé.
- Unárny `-` má vyššiu prioritu ako `+`, ale nižšiu ako `^`.

**Výsledný AST:**

```
             Add
            /    \
      Negate      Multiply
         |       /        \
      Num(2)  Num(3)      Power
                         /      \
                     Num(4)   Num(2)
```

**Vyhodnotenie (postorder — deti pred rodičom):**
1. `Power(4, 2)` → `16`
2. `Multiply(3, 16)` → `48`
3. `Negate(2)` → `-2`
4. `Add(-2, 48)` → `46` ✅

### Príklad 3: `x > 10 AND x < 50`

**Očakávaný výsledok:** `true` ak `10 < x < 50`, inak `false`.

**Predpokladajme `x = 25`.**

**Kľúčové pozorovania:**
- `AND` má nižšiu prioritu ako porovnania → `>` a `<` sa vyhodnotia ako prvé.
- `x` je identifikátor (premenná) — evaluator ho vyhľadá v tabuľke premenných.

**Výsledný AST:**

```
              And
            /      \
        Greater     Less
        /    \     /    \
      Var(x) Num(10) Var(x) Num(50)
```

**Vyhodnotenie:**
1. `Var(x)` → `25`
2. `Greater(25, 10)` → `true`
3. `Var(x)` → `25`
4. `Less(25, 50)` → `true`
5. `And(true, true)` → `true` ✅

---

## 9. Bežné chyby a okrajové prípady

### 9.1 Reťazené porovnania zakázané

`1 < x < 10` je **syntaktická chyba**. Používateľ musí napísať
`1 < x AND x < 10`.

### 9.2 `NOT NOT x` zakázané

V MVP. Používateľ musí napísať `NOT (NOT x)`.

### 9.3 Prázdny vstup

Prázdny reťazec je neplatný výraz — parser nahlási chybu "expected expression".

### 9.4 Nezatvorené zátvorky

`(3 + 5` → chyba: "expected `)`".

### 9.5 Nečakané tokeny

`3 + + 5` → chyba: "expected expression after `+`".

### 9.6 Implicitné násobenie neexistuje

`2x` je lexovaný ako `NUM(2)` a `IDENT(x)` → syntaktická chyba. Používateľ musí
napísať `2 * x`.

### 9.7 Unárny plus

`+5` nie je explicitne v gramatike. **Dizajnové rozhodnutie:** podporovať ho,
alebo nie?

> V MVP **nepodporujeme** unárny `+`. Je to zbytočný syntaktický šum. Ak sa
> niekedy rozšíri, stačí upraviť pravidlo `unary` na `[ "+" | "-" ] , primary`.

---

## 10. Čo jazyk nepodporuje

Zámerne vynechané z MVP (možné budúce rozšírenia pre bakalársku prácu):

- **Reťazcové literály** (`"hello"`) a ich operácie.
- **Polia a zoznamy** (`[1, 2, 3]`).
- **Užívateľom definované funkcie** (`f(x) = x * 2`).
- **Premenné definované v rámci výrazu** (`let x = 5 in x + 1`).
- **Podmienené výrazy** (`if x > 0 then x else -x`).
- **Bitové operátory** (`&`, `|`, `<<`, `>>`).
- **Unárny `+`**.
- **Reťazené porovnania** (`1 < x < 10`).
- **Dvojité negácie** bez zátvoriek (`NOT NOT x`).
- **Vedecká notácia** (`1e10`).
- **Hexadecimálne literály** (`0xFF`).

---

## Appendix A: Celá gramatika na jednom mieste

Pre rýchlu referenciu — všetky pravidlá bez komentárov:

```ebnf
(* === Lexikálna gramatika === *)

whitespace     = " " | "\t" | "\r" | "\n" ;
digit          = "0" | "1" | "2" | "3" | "4" | "5" | "6" | "7" | "8" | "9" ;
letter         = "A" | "B" | ... | "Z" | "a" | "b" | ... | "z" ;

integer_part   = digit , { digit } ;
fraction_part  = "." , digit , { digit } ;
number         = integer_part , [ fraction_part ] ;

identifier     = ( letter | "_" ) , { letter | digit | "_" } ;

keyword        = "true" | "false"
               | "AND"  | "OR"   | "NOT"
               | "and"  | "or"   | "not" ;


(* === Syntaktická gramatika === *)

expression     = or_expr ;

or_expr        = and_expr , { ( "OR" | "or" | "||" ) , and_expr } ;

and_expr       = not_expr , { ( "AND" | "and" | "&&" ) , not_expr } ;

not_expr       = [ ( "NOT" | "not" | "!" ) ] , comparison ;

comparison     = additive , [ comp_op , additive ] ;
comp_op        = "==" | "!=" | "<" | "<=" | ">" | ">=" ;

additive       = multiplicative , { ( "+" | "-" ) , multiplicative } ;

multiplicative = power , { ( "*" | "/" | "%" ) , power } ;

power          = unary , [ "^" , power ] ;

unary          = [ "-" ] , primary ;

primary        = number
               | "true"
               | "false"
               | function_call
               | identifier
               | "(" , expression , ")" ;

function_call  = identifier , "(" , [ argument_list ] , ")" ;
argument_list  = expression , { "," , expression } ;
```

---

## História verzií

| Verzia | Dátum        | Zmeny                                  |
|--------|--------------|----------------------------------------|
| 0.1    | Apríl 2026   | Pôvodná MVP špecifikácia.              |
