# Segmentation Manual

Application console C# pour gérer la visibilité des champs dans l'API de segmentation.

## Fonctionnalités

- ✅ Hide/Unhide des champs via l'API
- ✅ Configuration via fichier JSON
- ✅ Support de multiples DataSources
- ✅ Gestion des erreurs et retry
- ✅ Rapport détaillé des opérations

## Structure du projet

```
segmentation-manual/
├── Models/
│   └── FieldVisibilityRequest.cs    # Modèles de données
├── Services/
│   ├── ApiClient.cs                 # Client HTTP pour l'API
│   └── VisibilityService.cs         # Service de gestion de visibilité
├── Program.cs                        # Point d'entrée
├── config.json                       # Fichier de configuration exemple
├── config.json.example              # Exemple de configuration
├── SegmentationManual.sln           # Fichier solution
├── segmentation-manual.csproj       # Fichier projet
└── README.md                         # Documentation
```

## Configuration

Créez un fichier `config.json` avec la structure suivante:

```json
{
  "baseUrl": "https://sgmt-segmentation-{tenant}.cibe-prd-ee-sg.circus.be",
  "dataProducts": [
    {
      "dataSourceId": "votre-datasource-id",
      "fieldsToHide": [
        "champ1",
        "champ2"
      ],
      "fieldsToUnhide": [
        "champ3"
      ]
    }
  ]
}
```

### Paramètres

- `baseUrl`: URL de base de l'API (peut être surchargée via `--url`)
- `dataProducts`: Liste des DataSources à traiter
  - `dataSourceId`: Identifiant de la DataSource
  - `fieldsToHide`: Liste des champs à cacher
  - `fieldsToUnhide`: Liste des champs à rendre visibles

## Utilisation

### Compilation

```bash
dotnet build
```

### Exécution

```bash
# Utiliser la config.json par défaut
dotnet run

# Spécifier une URL différente
dotnet run -- --url https://sgmt-segmentation-777.cibe-prd-ee-sg.circus.be

# Utiliser un fichier de config personnalisé
dotnet run -- --config my-config.json

# Combinaison
dotnet run -- --url https://sgmt-segmentation-777.cibe-prd-ee-sg.circus.be --config my-config.json
```

### Exécution du binaire

```bash
# Après compilation (Release)
.\bin\Release\net10.0\segmentation-manual.exe --url https://sgmt-segmentation-777.cibe-prd-ee-sg.circus.be
```

## Endpoints API utilisés

- **Hide**: `POST /api/DataSources/{dataSourceId}/Fields/{fieldName}/Visibility/Hide`
- **Unhide**: `POST /api/DataSources/{dataSourceId}/Fields/{fieldName}/Visibility/Unhide`

## Exemples de tenants

- `777`: `https://sgmt-segmentation-777.cibe-prd-ee-sg.circus.be`
- `cibe`: `https://sgmt-segmentation-cibe.cibe-prd-ee-sg.circus.be`
- `du`: `https://sgmt-segmentation-du.cibe-prd-ee-sg.circus.be`

## Gestion des erreurs

L'application gère automatiquement:
- Les timeouts (5 minutes par défaut)
- Les erreurs HTTP
- Les erreurs de réseau
- Affiche un rapport détaillé des succès/échecs

## Notes

- Un délai de 500ms est ajouté entre chaque requête pour éviter de surcharger l'API
- Le timeout est configuré à 5 minutes par requête
- Toutes les opérations sont loggées dans la console
