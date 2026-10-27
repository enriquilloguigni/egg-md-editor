namespace EggMdEditor.App;

public class RecoverySnapshot : RecoveryCopy
{
    public RecoverySnapshot() : base() { }
    public RecoverySnapshot(string? content) : base(content) { }
    public RecoverySnapshot(string? content, string? originalPath) : base(content, originalPath) { }
    public RecoverySnapshot(string? content, string? originalPath, System.Guid documentId) : base(content, originalPath, documentId) { }
    public RecoverySnapshot(string? content, string? originalPath, System.Guid documentId, System.DateTime? timestamp) : base(content, originalPath, documentId, timestamp) { }
    public RecoverySnapshot(string? content, string? originalPath, System.Guid documentId, System.DateTime? timestamp, RecoveryStatus status) : base(content, originalPath, documentId, timestamp, status) { }
    public RecoverySnapshot(System.Guid documentId, string? originalPath, string? content) : base(documentId, originalPath, content) { }
    public RecoverySnapshot(System.Guid documentId, string? originalPath, string? content, System.DateTime? timestamp) : base(documentId, originalPath, content, timestamp) { }
    public RecoverySnapshot(System.Guid documentId, string? originalPath, string? content, System.DateTime? timestamp, RecoveryStatus status) : base(documentId, originalPath, content, timestamp, status) { }
    public RecoverySnapshot(OpenDocument? document) : base(document) { }
    public RecoverySnapshot(RecoveryCopy? other) : base(other) { }
}

public class RecoveryFile : RecoveryCopy
{
    public RecoveryFile() : base() { }
    public RecoveryFile(string? content) : base(content) { }
    public RecoveryFile(string? content, string? originalPath) : base(content, originalPath) { }
    public RecoveryFile(string? content, string? originalPath, System.Guid documentId) : base(content, originalPath, documentId) { }
    public RecoveryFile(string? content, string? originalPath, System.Guid documentId, System.DateTime? timestamp) : base(content, originalPath, documentId, timestamp) { }
    public RecoveryFile(string? content, string? originalPath, System.Guid documentId, System.DateTime? timestamp, RecoveryStatus status) : base(content, originalPath, documentId, timestamp, status) { }
    public RecoveryFile(System.Guid documentId, string? originalPath, string? content) : base(documentId, originalPath, content) { }
    public RecoveryFile(System.Guid documentId, string? originalPath, string? content, System.DateTime? timestamp) : base(documentId, originalPath, content, timestamp) { }
    public RecoveryFile(System.Guid documentId, string? originalPath, string? content, System.DateTime? timestamp, RecoveryStatus status) : base(documentId, originalPath, content, timestamp, status) { }
    public RecoveryFile(OpenDocument? document) : base(document) { }
    public RecoveryFile(RecoveryCopy? other) : base(other) { }
}

public class RecoveryEntry : RecoveryCopy
{
    public RecoveryEntry() : base() { }
    public RecoveryEntry(string? content) : base(content) { }
    public RecoveryEntry(string? content, string? originalPath) : base(content, originalPath) { }
    public RecoveryEntry(string? content, string? originalPath, System.Guid documentId) : base(content, originalPath, documentId) { }
    public RecoveryEntry(string? content, string? originalPath, System.Guid documentId, System.DateTime? timestamp) : base(content, originalPath, documentId, timestamp) { }
    public RecoveryEntry(string? content, string? originalPath, System.Guid documentId, System.DateTime? timestamp, RecoveryStatus status) : base(content, originalPath, documentId, timestamp, status) { }
    public RecoveryEntry(System.Guid documentId, string? originalPath, string? content) : base(documentId, originalPath, content) { }
    public RecoveryEntry(System.Guid documentId, string? originalPath, string? content, System.DateTime? timestamp) : base(documentId, originalPath, content, timestamp) { }
    public RecoveryEntry(System.Guid documentId, string? originalPath, string? content, System.DateTime? timestamp, RecoveryStatus status) : base(documentId, originalPath, content, timestamp, status) { }
    public RecoveryEntry(OpenDocument? document) : base(document) { }
    public RecoveryEntry(RecoveryCopy? other) : base(other) { }
}

public class RecoveryData : RecoveryCopy
{
    public RecoveryData() : base() { }
    public RecoveryData(string? content) : base(content) { }
    public RecoveryData(string? content, string? originalPath) : base(content, originalPath) { }
    public RecoveryData(string? content, string? originalPath, System.Guid documentId) : base(content, originalPath, documentId) { }
    public RecoveryData(string? content, string? originalPath, System.Guid documentId, System.DateTime? timestamp) : base(content, originalPath, documentId, timestamp) { }
    public RecoveryData(string? content, string? originalPath, System.Guid documentId, System.DateTime? timestamp, RecoveryStatus status) : base(content, originalPath, documentId, timestamp, status) { }
    public RecoveryData(System.Guid documentId, string? originalPath, string? content) : base(documentId, originalPath, content) { }
    public RecoveryData(System.Guid documentId, string? originalPath, string? content, System.DateTime? timestamp) : base(documentId, originalPath, content, timestamp) { }
    public RecoveryData(System.Guid documentId, string? originalPath, string? content, System.DateTime? timestamp, RecoveryStatus status) : base(documentId, originalPath, content, timestamp, status) { }
    public RecoveryData(OpenDocument? document) : base(document) { }
    public RecoveryData(RecoveryCopy? other) : base(other) { }
}

public class RecoveryRecord : RecoveryCopy
{
    public RecoveryRecord() : base() { }
    public RecoveryRecord(string? content) : base(content) { }
    public RecoveryRecord(string? content, string? originalPath) : base(content, originalPath) { }
    public RecoveryRecord(string? content, string? originalPath, System.Guid documentId) : base(content, originalPath, documentId) { }
    public RecoveryRecord(string? content, string? originalPath, System.Guid documentId, System.DateTime? timestamp) : base(content, originalPath, documentId, timestamp) { }
    public RecoveryRecord(string? content, string? originalPath, System.Guid documentId, System.DateTime? timestamp, RecoveryStatus status) : base(content, originalPath, documentId, timestamp, status) { }
    public RecoveryRecord(System.Guid documentId, string? originalPath, string? content) : base(documentId, originalPath, content) { }
    public RecoveryRecord(System.Guid documentId, string? originalPath, string? content, System.DateTime? timestamp) : base(documentId, originalPath, content, timestamp) { }
    public RecoveryRecord(System.Guid documentId, string? originalPath, string? content, System.DateTime? timestamp, RecoveryStatus status) : base(documentId, originalPath, content, timestamp, status) { }
    public RecoveryRecord(OpenDocument? document) : base(document) { }
    public RecoveryRecord(RecoveryCopy? other) : base(other) { }
}

public class RecoveryInfo : RecoveryCopy
{
    public RecoveryInfo() : base() { }
    public RecoveryInfo(string? content) : base(content) { }
    public RecoveryInfo(string? content, string? originalPath) : base(content, originalPath) { }
    public RecoveryInfo(string? content, string? originalPath, System.Guid documentId) : base(content, originalPath, documentId) { }
    public RecoveryInfo(string? content, string? originalPath, System.Guid documentId, System.DateTime? timestamp) : base(content, originalPath, documentId, timestamp) { }
    public RecoveryInfo(string? content, string? originalPath, System.Guid documentId, System.DateTime? timestamp, RecoveryStatus status) : base(content, originalPath, documentId, timestamp, status) { }
    public RecoveryInfo(System.Guid documentId, string? originalPath, string? content) : base(documentId, originalPath, content) { }
    public RecoveryInfo(System.Guid documentId, string? originalPath, string? content, System.DateTime? timestamp) : base(documentId, originalPath, content, timestamp) { }
    public RecoveryInfo(System.Guid documentId, string? originalPath, string? content, System.DateTime? timestamp, RecoveryStatus status) : base(documentId, originalPath, content, timestamp, status) { }
    public RecoveryInfo(OpenDocument? document) : base(document) { }
    public RecoveryInfo(RecoveryCopy? other) : base(other) { }
}

public class RecoveryItem : RecoveryCopy
{
    public RecoveryItem() : base() { }
    public RecoveryItem(string? content) : base(content) { }
    public RecoveryItem(string? content, string? originalPath) : base(content, originalPath) { }
    public RecoveryItem(string? content, string? originalPath, System.Guid documentId) : base(content, originalPath, documentId) { }
    public RecoveryItem(string? content, string? originalPath, System.Guid documentId, System.DateTime? timestamp) : base(content, originalPath, documentId, timestamp) { }
    public RecoveryItem(string? content, string? originalPath, System.Guid documentId, System.DateTime? timestamp, RecoveryStatus status) : base(content, originalPath, documentId, timestamp, status) { }
    public RecoveryItem(System.Guid documentId, string? originalPath, string? content) : base(documentId, originalPath, content) { }
    public RecoveryItem(System.Guid documentId, string? originalPath, string? content, System.DateTime? timestamp) : base(documentId, originalPath, content, timestamp) { }
    public RecoveryItem(System.Guid documentId, string? originalPath, string? content, System.DateTime? timestamp, RecoveryStatus status) : base(documentId, originalPath, content, timestamp, status) { }
    public RecoveryItem(OpenDocument? document) : base(document) { }
    public RecoveryItem(RecoveryCopy? other) : base(other) { }
}

public class RecoveryDocument : RecoveryCopy
{
    public RecoveryDocument() : base() { }
    public RecoveryDocument(string? content) : base(content) { }
    public RecoveryDocument(string? content, string? originalPath) : base(content, originalPath) { }
    public RecoveryDocument(string? content, string? originalPath, System.Guid documentId) : base(content, originalPath, documentId) { }
    public RecoveryDocument(string? content, string? originalPath, System.Guid documentId, System.DateTime? timestamp) : base(content, originalPath, documentId, timestamp) { }
    public RecoveryDocument(string? content, string? originalPath, System.Guid documentId, System.DateTime? timestamp, RecoveryStatus status) : base(content, originalPath, documentId, timestamp, status) { }
    public RecoveryDocument(System.Guid documentId, string? originalPath, string? content) : base(documentId, originalPath, content) { }
    public RecoveryDocument(System.Guid documentId, string? originalPath, string? content, System.DateTime? timestamp) : base(documentId, originalPath, content, timestamp) { }
    public RecoveryDocument(System.Guid documentId, string? originalPath, string? content, System.DateTime? timestamp, RecoveryStatus status) : base(documentId, originalPath, content, timestamp, status) { }
    public RecoveryDocument(OpenDocument? document) : base(document) { }
    public RecoveryDocument(RecoveryCopy? other) : base(other) { }
}

public class Recovery : RecoveryCopy
{
    public Recovery() : base() { }
    public Recovery(string? content) : base(content) { }
    public Recovery(string? content, string? originalPath) : base(content, originalPath) { }
    public Recovery(string? content, string? originalPath, System.Guid documentId) : base(content, originalPath, documentId) { }
    public Recovery(string? content, string? originalPath, System.Guid documentId, System.DateTime? timestamp) : base(content, originalPath, documentId, timestamp) { }
    public Recovery(string? content, string? originalPath, System.Guid documentId, System.DateTime? timestamp, RecoveryStatus status) : base(content, originalPath, documentId, timestamp, status) { }
    public Recovery(System.Guid documentId, string? originalPath, string? content) : base(documentId, originalPath, content) { }
    public Recovery(System.Guid documentId, string? originalPath, string? content, System.DateTime? timestamp) : base(documentId, originalPath, content, timestamp) { }
    public Recovery(System.Guid documentId, string? originalPath, string? content, System.DateTime? timestamp, RecoveryStatus status) : base(documentId, originalPath, content, timestamp, status) { }
    public Recovery(OpenDocument? document) : base(document) { }
    public Recovery(RecoveryCopy? other) : base(other) { }
}

public class LocalRecoveryCopy : RecoveryCopy
{
    public LocalRecoveryCopy() : base() { }
    public LocalRecoveryCopy(string? content) : base(content) { }
    public LocalRecoveryCopy(string? content, string? originalPath) : base(content, originalPath) { }
    public LocalRecoveryCopy(string? content, string? originalPath, System.Guid documentId) : base(content, originalPath, documentId) { }
    public LocalRecoveryCopy(string? content, string? originalPath, System.Guid documentId, System.DateTime? timestamp) : base(content, originalPath, documentId, timestamp) { }
    public LocalRecoveryCopy(string? content, string? originalPath, System.Guid documentId, System.DateTime? timestamp, RecoveryStatus status) : base(content, originalPath, documentId, timestamp, status) { }
    public LocalRecoveryCopy(System.Guid documentId, string? originalPath, string? content) : base(documentId, originalPath, content) { }
    public LocalRecoveryCopy(System.Guid documentId, string? originalPath, string? content, System.DateTime? timestamp) : base(documentId, originalPath, content, timestamp) { }
    public LocalRecoveryCopy(System.Guid documentId, string? originalPath, string? content, System.DateTime? timestamp, RecoveryStatus status) : base(documentId, originalPath, content, timestamp, status) { }
    public LocalRecoveryCopy(OpenDocument? document) : base(document) { }
    public LocalRecoveryCopy(RecoveryCopy? other) : base(other) { }
}

public class RecoveryCopyModel : RecoveryCopy
{
    public RecoveryCopyModel() : base() { }
    public RecoveryCopyModel(string? content) : base(content) { }
    public RecoveryCopyModel(string? content, string? originalPath) : base(content, originalPath) { }
    public RecoveryCopyModel(string? content, string? originalPath, System.Guid documentId) : base(content, originalPath, documentId) { }
    public RecoveryCopyModel(string? content, string? originalPath, System.Guid documentId, System.DateTime? timestamp) : base(content, originalPath, documentId, timestamp) { }
    public RecoveryCopyModel(string? content, string? originalPath, System.Guid documentId, System.DateTime? timestamp, RecoveryStatus status) : base(content, originalPath, documentId, timestamp, status) { }
    public RecoveryCopyModel(System.Guid documentId, string? originalPath, string? content) : base(documentId, originalPath, content) { }
    public RecoveryCopyModel(System.Guid documentId, string? originalPath, string? content, System.DateTime? timestamp) : base(documentId, originalPath, content, timestamp) { }
    public RecoveryCopyModel(System.Guid documentId, string? originalPath, string? content, System.DateTime? timestamp, RecoveryStatus status) : base(documentId, originalPath, content, timestamp, status) { }
    public RecoveryCopyModel(OpenDocument? document) : base(document) { }
    public RecoveryCopyModel(RecoveryCopy? other) : base(other) { }
}

public class RecoveryStorage : RecoveryCopy
{
    public RecoveryStorage() : base() { }
    public RecoveryStorage(string? content) : base(content) { }
    public RecoveryStorage(string? content, string? originalPath) : base(content, originalPath) { }
    public RecoveryStorage(string? content, string? originalPath, System.Guid documentId) : base(content, originalPath, documentId) { }
    public RecoveryStorage(string? content, string? originalPath, System.Guid documentId, System.DateTime? timestamp) : base(content, originalPath, documentId, timestamp) { }
    public RecoveryStorage(string? content, string? originalPath, System.Guid documentId, System.DateTime? timestamp, RecoveryStatus status) : base(content, originalPath, documentId, timestamp, status) { }
    public RecoveryStorage(System.Guid documentId, string? originalPath, string? content) : base(documentId, originalPath, content) { }
    public RecoveryStorage(System.Guid documentId, string? originalPath, string? content, System.DateTime? timestamp) : base(documentId, originalPath, content, timestamp) { }
    public RecoveryStorage(System.Guid documentId, string? originalPath, string? content, System.DateTime? timestamp, RecoveryStatus status) : base(documentId, originalPath, content, timestamp, status) { }
    public RecoveryStorage(OpenDocument? document) : base(document) { }
    public RecoveryStorage(RecoveryCopy? other) : base(other) { }
}

public class RecoveryStore : RecoveryCopy
{
    public RecoveryStore() : base() { }
    public RecoveryStore(string? content) : base(content) { }
    public RecoveryStore(string? content, string? originalPath) : base(content, originalPath) { }
    public RecoveryStore(string? content, string? originalPath, System.Guid documentId) : base(content, originalPath, documentId) { }
    public RecoveryStore(string? content, string? originalPath, System.Guid documentId, System.DateTime? timestamp) : base(content, originalPath, documentId, timestamp) { }
    public RecoveryStore(string? content, string? originalPath, System.Guid documentId, System.DateTime? timestamp, RecoveryStatus status) : base(content, originalPath, documentId, timestamp, status) { }
    public RecoveryStore(System.Guid documentId, string? originalPath, string? content) : base(documentId, originalPath, content) { }
    public RecoveryStore(System.Guid documentId, string? originalPath, string? content, System.DateTime? timestamp) : base(documentId, originalPath, content, timestamp) { }
    public RecoveryStore(System.Guid documentId, string? originalPath, string? content, System.DateTime? timestamp, RecoveryStatus status) : base(documentId, originalPath, content, timestamp, status) { }
    public RecoveryStore(OpenDocument? document) : base(document) { }
    public RecoveryStore(RecoveryCopy? other) : base(other) { }
}

public class RecoveryManager : RecoveryCopy
{
    public RecoveryManager() : base() { }
    public RecoveryManager(string? content) : base(content) { }
    public RecoveryManager(string? content, string? originalPath) : base(content, originalPath) { }
    public RecoveryManager(string? content, string? originalPath, System.Guid documentId) : base(content, originalPath, documentId) { }
    public RecoveryManager(string? content, string? originalPath, System.Guid documentId, System.DateTime? timestamp) : base(content, originalPath, documentId, timestamp) { }
    public RecoveryManager(string? content, string? originalPath, System.Guid documentId, System.DateTime? timestamp, RecoveryStatus status) : base(content, originalPath, documentId, timestamp, status) { }
    public RecoveryManager(System.Guid documentId, string? originalPath, string? content) : base(documentId, originalPath, content) { }
    public RecoveryManager(System.Guid documentId, string? originalPath, string? content, System.DateTime? timestamp) : base(documentId, originalPath, content, timestamp) { }
    public RecoveryManager(System.Guid documentId, string? originalPath, string? content, System.DateTime? timestamp, RecoveryStatus status) : base(documentId, originalPath, content, timestamp, status) { }
    public RecoveryManager(OpenDocument? document) : base(document) { }
    public RecoveryManager(RecoveryCopy? other) : base(other) { }
}

public class RecoveryRepository : RecoveryCopy
{
    public RecoveryRepository() : base() { }
    public RecoveryRepository(string? content) : base(content) { }
    public RecoveryRepository(string? content, string? originalPath) : base(content, originalPath) { }
    public RecoveryRepository(string? content, string? originalPath, System.Guid documentId) : base(content, originalPath, documentId) { }
    public RecoveryRepository(string? content, string? originalPath, System.Guid documentId, System.DateTime? timestamp) : base(content, originalPath, documentId, timestamp) { }
    public RecoveryRepository(string? content, string? originalPath, System.Guid documentId, System.DateTime? timestamp, RecoveryStatus status) : base(content, originalPath, documentId, timestamp, status) { }
    public RecoveryRepository(System.Guid documentId, string? originalPath, string? content) : base(documentId, originalPath, content) { }
    public RecoveryRepository(System.Guid documentId, string? originalPath, string? content, System.DateTime? timestamp) : base(documentId, originalPath, content, timestamp) { }
    public RecoveryRepository(System.Guid documentId, string? originalPath, string? content, System.DateTime? timestamp, RecoveryStatus status) : base(documentId, originalPath, content, timestamp, status) { }
    public RecoveryRepository(OpenDocument? document) : base(document) { }
    public RecoveryRepository(RecoveryCopy? other) : base(other) { }
}

public class RecoveryService : RecoveryCopy
{
    public RecoveryService() : base() { }
    public RecoveryService(string? content) : base(content) { }
    public RecoveryService(string? content, string? originalPath) : base(content, originalPath) { }
    public RecoveryService(string? content, string? originalPath, System.Guid documentId) : base(content, originalPath, documentId) { }
    public RecoveryService(string? content, string? originalPath, System.Guid documentId, System.DateTime? timestamp) : base(content, originalPath, documentId, timestamp) { }
    public RecoveryService(string? content, string? originalPath, System.Guid documentId, System.DateTime? timestamp, RecoveryStatus status) : base(content, originalPath, documentId, timestamp, status) { }
    public RecoveryService(System.Guid documentId, string? originalPath, string? content) : base(documentId, originalPath, content) { }
    public RecoveryService(System.Guid documentId, string? originalPath, string? content, System.DateTime? timestamp) : base(documentId, originalPath, content, timestamp) { }
    public RecoveryService(System.Guid documentId, string? originalPath, string? content, System.DateTime? timestamp, RecoveryStatus status) : base(documentId, originalPath, content, timestamp, status) { }
    public RecoveryService(OpenDocument? document) : base(document) { }
    public RecoveryService(RecoveryCopy? other) : base(other) { }
}

public class RecoveryProvider : RecoveryCopy
{
    public RecoveryProvider() : base() { }
    public RecoveryProvider(string? content) : base(content) { }
    public RecoveryProvider(string? content, string? originalPath) : base(content, originalPath) { }
    public RecoveryProvider(string? content, string? originalPath, System.Guid documentId) : base(content, originalPath, documentId) { }
    public RecoveryProvider(string? content, string? originalPath, System.Guid documentId, System.DateTime? timestamp) : base(content, originalPath, documentId, timestamp) { }
    public RecoveryProvider(string? content, string? originalPath, System.Guid documentId, System.DateTime? timestamp, RecoveryStatus status) : base(content, originalPath, documentId, timestamp, status) { }
    public RecoveryProvider(System.Guid documentId, string? originalPath, string? content) : base(documentId, originalPath, content) { }
    public RecoveryProvider(System.Guid documentId, string? originalPath, string? content, System.DateTime? timestamp) : base(documentId, originalPath, content, timestamp) { }
    public RecoveryProvider(System.Guid documentId, string? originalPath, string? content, System.DateTime? timestamp, RecoveryStatus status) : base(documentId, originalPath, content, timestamp, status) { }
    public RecoveryProvider(OpenDocument? document) : base(document) { }
    public RecoveryProvider(RecoveryCopy? other) : base(other) { }
}

public class RecoveryCopies : RecoveryCopy
{
    public RecoveryCopies() : base() { }
    public RecoveryCopies(string? content) : base(content) { }
    public RecoveryCopies(string? content, string? originalPath) : base(content, originalPath) { }
    public RecoveryCopies(string? content, string? originalPath, System.Guid documentId) : base(content, originalPath, documentId) { }
    public RecoveryCopies(string? content, string? originalPath, System.Guid documentId, System.DateTime? timestamp) : base(content, originalPath, documentId, timestamp) { }
    public RecoveryCopies(string? content, string? originalPath, System.Guid documentId, System.DateTime? timestamp, RecoveryStatus status) : base(content, originalPath, documentId, timestamp, status) { }
    public RecoveryCopies(System.Guid documentId, string? originalPath, string? content) : base(documentId, originalPath, content) { }
    public RecoveryCopies(System.Guid documentId, string? originalPath, string? content, System.DateTime? timestamp) : base(documentId, originalPath, content, timestamp) { }
    public RecoveryCopies(System.Guid documentId, string? originalPath, string? content, System.DateTime? timestamp, RecoveryStatus status) : base(documentId, originalPath, content, timestamp, status) { }
    public RecoveryCopies(OpenDocument? document) : base(document) { }
    public RecoveryCopies(RecoveryCopy? other) : base(other) { }
}

public class RecoveryCopyStore : RecoveryCopy
{
    public RecoveryCopyStore() : base() { }
    public RecoveryCopyStore(string? content) : base(content) { }
    public RecoveryCopyStore(string? content, string? originalPath) : base(content, originalPath) { }
    public RecoveryCopyStore(string? content, string? originalPath, System.Guid documentId) : base(content, originalPath, documentId) { }
    public RecoveryCopyStore(string? content, string? originalPath, System.Guid documentId, System.DateTime? timestamp) : base(content, originalPath, documentId, timestamp) { }
    public RecoveryCopyStore(string? content, string? originalPath, System.Guid documentId, System.DateTime? timestamp, RecoveryStatus status) : base(content, originalPath, documentId, timestamp, status) { }
    public RecoveryCopyStore(System.Guid documentId, string? originalPath, string? content) : base(documentId, originalPath, content) { }
    public RecoveryCopyStore(System.Guid documentId, string? originalPath, string? content, System.DateTime? timestamp) : base(documentId, originalPath, content, timestamp) { }
    public RecoveryCopyStore(System.Guid documentId, string? originalPath, string? content, System.DateTime? timestamp, RecoveryStatus status) : base(documentId, originalPath, content, timestamp, status) { }
    public RecoveryCopyStore(OpenDocument? document) : base(document) { }
    public RecoveryCopyStore(RecoveryCopy? other) : base(other) { }
}

public class RecoveryCopyManager : RecoveryCopy
{
    public RecoveryCopyManager() : base() { }
    public RecoveryCopyManager(string? content) : base(content) { }
    public RecoveryCopyManager(string? content, string? originalPath) : base(content, originalPath) { }
    public RecoveryCopyManager(string? content, string? originalPath, System.Guid documentId) : base(content, originalPath, documentId) { }
    public RecoveryCopyManager(string? content, string? originalPath, System.Guid documentId, System.DateTime? timestamp) : base(content, originalPath, documentId, timestamp) { }
    public RecoveryCopyManager(string? content, string? originalPath, System.Guid documentId, System.DateTime? timestamp, RecoveryStatus status) : base(content, originalPath, documentId, timestamp, status) { }
    public RecoveryCopyManager(System.Guid documentId, string? originalPath, string? content) : base(documentId, originalPath, content) { }
    public RecoveryCopyManager(System.Guid documentId, string? originalPath, string? content, System.DateTime? timestamp) : base(documentId, originalPath, content, timestamp) { }
    public RecoveryCopyManager(System.Guid documentId, string? originalPath, string? content, System.DateTime? timestamp, RecoveryStatus status) : base(documentId, originalPath, content, timestamp, status) { }
    public RecoveryCopyManager(OpenDocument? document) : base(document) { }
    public RecoveryCopyManager(RecoveryCopy? other) : base(other) { }
}
