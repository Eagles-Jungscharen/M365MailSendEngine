# M365MailSendEngine
Mailsend Engine, welche durch Einträge in einer Sharepoint Liste gesteuert wird und Attachments aus einer definierten Liste versende. Ink. QR Code Rechnungsanbindung.

## Prerequirments
- Sharepoint Space with MailDefinitionTable and IncomingMailList
- Application Registration
    - Mail.Send / application
    - Sites.ReadWrite.All / application
    - User.ReadBasic.All / application

## Installation
### Infrastruktur via Bicep und az CLI auf den Azure Tenant schreiben
az group create --name <resourceGroupName> --location <azureLocation>
az deployment group create --resource-group <resourceGroupName> --template-file main.bicep --mode Incremental --parameters tenantId=<tenantId> applicationId=<applicationId> applicationSecret=<application-secret> siteId=<site-id> definitionListId=<listid-for-definition> incomingMailListId=<listid-for-incomingmails> azureObjectIdForStoreUser=<azureid-user> qrCodeUrl=<url-code-qr> qrCodeSecret=<qr-code-secret>
