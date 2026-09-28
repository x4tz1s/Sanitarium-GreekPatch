# Sanitarium — Ελληνικό Patch / Greek Translation Patch

Ανεπίσημη πλήρης ελληνική μετάφραση για το **Sanitarium** (1998, έκδοση Steam): μενού, ρυθμίσεις, **όλοι οι διάλογοι**,
περιγραφές, σκέψεις του Μαξ, εφημερίδες και ημερολόγια, **υπότιτλοι των βίντεο** και τίτλοι τέλους (~3.900 κείμενα),
με **ελληνικές γραμματοσειρές** σχεδιασμένες στο ύφος του παιχνιδιού.

*Unofficial full Greek translation for **Sanitarium** (1998, Steam version): menus, options, all dialogue, descriptions,
newspapers and diaries, cinematic subtitles and credits (~3,900 strings), with Greek glyphs drawn in the game's own font style.*

## Πώς δουλεύει / How it works

Το πακέτο **δεν περιέχει αρχεία του παιχνιδιού**. Περιέχει μόνο τα κομμάτια που άλλαξαν (τα ελληνικά κείμενα και τις γραμματοσειρές)
και ένα μικρό εργαλείο που τα βάζει μέσα στα **δικά σου** αρχεία `Data\EN\RES.*`. Το εργαλείο ελέγχει ότι κάθε αρχείο είναι το αυθεντικό
πριν αλλάξει οτιδήποτε, ελέγχει ότι κάθε αποτέλεσμα είναι ακριβώς το αναμενόμενο και κρατά αντίγραφο ασφαλείας (`GreekPatch_Backup`
μέσα στον φάκελο του παιχνιδιού).

*No game data is included — only the changed resources (Greek text and fonts) and a small tool that puts them into **your own**
`Data\EN\RES.*` files. Every original file is verified first, every result is checked against the expected checksum, and a backup is kept.*

## Εγκατάσταση / Install (Windows)

Το mod έρχεται σε **δύο μέρη**: τα **δεδομένα μετάφρασης** (φάκελος `Data`, στο Nexus) και το **πρόγραμμα** που τα εφαρμόζει (εδώ, στο GitHub).

1. Κατέβασε το πρόγραμμα από τα [Releases](../../releases) (`SanitariumGreekPatcher_v1.0_Windows.zip`) και αποσυμπίεσέ το σε έναν φάκελο.
2. Κατέβασε τα δεδομένα από τη σελίδα του mod στο Nexus και αποσυμπίεσέ τα **στον ίδιο φάκελο**, ώστε δίπλα στο
   `SanitariumGreekPatcher.exe` να υπάρχει ο φάκελος `Data` (με μέσα το `manifest.json`). *(Ο φάκελος `Data` υπάρχει και σε αυτό το repo.)*
3. Κλείσε το παιχνίδι και τρέξε το **`Install.bat`** (διπλό κλικ). Βρίσκει μόνο του το Steam/GOG· αν όχι, θα σου ζητήσει τον φάκελο,
   π.χ. `C:\Program Files (x86)\Steam\steamapps\common\Sanitarium`.
4. Σε λίγα δευτερόλεπτα γράφει «Έτοιμο!». Ξεκίνα το παιχνίδι — τα ελληνικά εμφανίζονται αμέσως (και σε παλιά saves).
   Για υπότιτλους στους διαλόγους/βίντεο: Ρυθμίσεις κειμένου → Υπότιτλοι βίντεο / Υπότιτλοι διαλόγων → Ναι.

### Linux / Steam Deck (Proton)

Κατέβασε το `SanitariumGreekPatcher_v1.0_Linux.tar.gz`, βγάλ' το, βάλε δίπλα τον φάκελο `Data` και τρέξε `./SanitariumGreekPatcher`
(ή `--game "/path/to/steamapps/common/Sanitarium"`).

### Επιλογές / Options

```
Install.bat [--game "<φάκελος παιχνιδιού>"] [--data "<φάκελος Data>"] [--no-pause]            (Windows)
./SanitariumGreekPatcher [--game <φάκελος>] [--data <φάκελος>] [--restore] [--no-pause]       (Linux)
```

## Απεγκατάσταση / Uninstall

`Uninstall.bat` (Linux: `./SanitariumGreekPatcher --restore`): επαναφέρει τα αυθεντικά αρχεία από το `GreekPatch_Backup`.
Εναλλακτικά: Steam → *Verify integrity of game files*. Ενημερώσεις του παιχνιδιού ή «verify» αναιρούν το patch — τρέξε ξανά το εργαλείο.

## Συμβατότητα / Compatibility

Φτιάχτηκε και δοκιμάστηκε με την έκδοση Steam, σε Linux/Proton· το εργαλείο Windows δοκιμάστηκε με Wine. Η έκδοση GOG δεν έχει δοκιμαστεί.
Το εργαλείο σταματά με μήνυμα αν κάποιο αρχείο δεν είναι το αναμενόμενο (άλλη έκδοση ή ήδη τροποποιημένο) — δεν γράφει ποτέ πάνω σε άγνωστο περιεχόμενο.
Το αντίγραφο ασφαλείας πιάνει ~420 MB, γιατί οι γραμματοσειρές βρίσκονται μέσα στα μεγάλα αρχεία RES των κεφαλαίων.

*Made for the Steam version; GOG untested. The tool refuses to touch files that aren't the expected originals. The backup takes ~420 MB.*

## Σημειώσεις μετάφρασης / Translation notes

- Ανθρώπινη (όχι μηχανική) μετάφραση, με προσοχή στους γρίφους: ο συνδυασμός της κλειδαριάς, το σύνθημα του δέντρου,
  η σειρά των πολεμιστών, οι κωδικοί και η φράση «Η νιότη κρύβει το κλειδί της σωτηρίας» λειτουργούν κανονικά στα ελληνικά.
- Τα ονόματα γράφονται με ελληνικά γράμματα (Μαξ, Σάρα, Μόργκαν, Γκρίμγουολ, Όλμεκ, Κετσαλκόατλ…). Στους τίτλους τέλους τα ονόματα των
  δημιουργών μένουν λατινικά.
- Οι φωνές μένουν αγγλικές.

## Άδεια / License

Το εργαλείο: MIT (δες `LICENSE`). Το παιχνίδι και το αρχικό κείμενο ανήκουν στους δικαιούχους τους (DreamForge Intertainment / ASC Games) —
ανεπίσημη μετάφραση από θαυμαστή. *Tool: MIT. Game and original text © their respective owners — unofficial fan translation.*

Μετάφραση / Translation: **X4Tz1S**
