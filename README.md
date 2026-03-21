# M365MailSendEngine

Mailversand-Engine auf Azure Functions. Die Verarbeitung wird über Einträge in einer SharePoint-Liste gesteuert. Optional kann ein QR-Rechnungsbild generiert und der Mail als Attachment hinzugefügt werden.

## Überblick

- Timer-Trigger läuft jede Minute.
- Es werden nur Einträge mit Status `Draft` aus der Incoming-Liste verarbeitet.
- Für jeden Eintrag wird die passende Mail-Definition über den MailKey geladen.
- Attachments werden aus dem SharePoint-Drive geladen (Ordnername = MailKey).
- Nach erfolgreichem Versand wird der Status auf `Sent` gesetzt.

## Voraussetzungen

### 1) SharePoint

- Eine Site mit:
    - Liste für Mail-Definitionen
    - Liste für eingehende Mails
    - Dokumentbibliothek/Drive mit optionalen Attachment-Ordnern pro MailKey

### 2) App Registration (Microsoft Graph, Application Permissions)

- `Mail.Send`
- `Sites.ReadWrite.All`

Admin Consent ist erforderlich.

### 3) QR-Code Service (optional/fachlich erforderlich, falls QR genutzt wird)

- HTTP-Endpunkt, der `POST api/GenerateQRBill?png=1` akzeptiert
- Authentifizierung über Header `x-functions-key`

## Infrastruktur-Deployment (Bicep)

```bash
az group create --name <resourceGroupName> --location <azureLocation>

az deployment group create \
    --resource-group <resourceGroupName> \
    --template-file main.bicep \
    --mode Incremental \
    --parameters \
        tenantId=<tenantId> \
        applicationId=<applicationId> \
        applicationSecret=<application-secret> \
        siteId=<site-id> \
        definitionListId=<listid-for-definition> \
        incomingMailListId=<listid-for-incomingmails> \
        azureObjectIdForStoreUser=<azureid-user> \
        qrCodeUrl=<url-code-qr> \
        qrCodeSecret=<qr-code-secret>
```

Die Parameter werden als App Settings der Function App gesetzt, sensible Werte über Key Vault referenziert.

## SharePoint-Datenmodell

Wichtig: Es werden interne Feldnamen verwendet. Diese müssen exakt so existieren.

### Incoming-Liste (MailRequest)

- `Title` (MailKey)
- `email`
- `firstname`
- `lastname`
- `street`
- `postalcode`
- `town`
- `additinonalinfos` (Hinweis: Schreibweise ist aktuell absichtlich so im Code)
- `countrycode`
- `status` (`Draft`/`Sent`)
- `amount`
- `currency`
- `infotext`

### Definition-Liste (MailDefinition)

- `Title` (Suche über MailKey)
- `mailkey`
- `mailsubject`
- `mailtext`
- `replyto` (wird als sendende User-Mailbox für Graph `users/{id-or-upn}/sendMail` verwendet)
- `qrbill`
- `iban`
- `qrname`
- `qrstreet`
- `qrhousenumber`
- `qrpostalcode`
- `qrtown`
- `qrcountrycode`

## Platzhalter im Mailtext

Folgende Platzhalter werden im Mailtext ersetzt:

- `{firstname}`
- `{lastname}`
- `{additionalinfos}`
- `{amount}`
- `{currency}`
- `{address}`
- `{postalCode}`
- `{town}`
- `{infotext}`

## Lokaler Start

```bash
dotnet build
func host start
```

Alternativ über die VS Code Tasks (`build (functions)` und `func: 4`).

