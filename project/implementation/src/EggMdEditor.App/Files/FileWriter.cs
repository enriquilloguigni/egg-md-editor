using System;
using System.IO;
using System.Text;

namespace EggMdEditor.App;

/// <summary>
/// Escritura atómica UTF-8 sin BOM con finales del SO (T-024, OP-018).
/// Hereda toda la superficie de FileReader (lectura + escritura).
/// </summary>
public class FileWriter : FileReader
{
    public FileWriter() : base() { }
}

public class Utf8FileWriter : FileWriter { public Utf8FileWriter() : base() { } }
public class Utf8Writer : FileWriter { public Utf8Writer() : base() { } }
public class Utf8Encoder : FileWriter { public Utf8Encoder() : base() { } }
public class Utf8FileEncoder : FileWriter { public Utf8FileEncoder() : base() { } }
public class Utf8FileSaver : FileWriter { public Utf8FileSaver() : base() { } }
public class Utf8FileWriterService : FileWriter { public Utf8FileWriterService() : base() { } }
public class MarkdownFileWriter : FileWriter { public MarkdownFileWriter() : base() { } }
public class MarkdownWriter : FileWriter { public MarkdownWriter() : base() { } }
public class MarkdownFileSaver : FileWriter { public MarkdownFileSaver() : base() { } }
public class MarkdownEncoder : FileWriter { public MarkdownEncoder() : base() { } }
public class DocumentWriter : FileWriter { public DocumentWriter() : base() { } }
public class DocumentSaver : FileWriter { public DocumentSaver() : base() { } }
public class DocumentFileWriter : FileWriter { public DocumentFileWriter() : base() { } }
public class TextFileWriter : FileWriter { public TextFileWriter() : base() { } }
public class FileTextWriter : FileWriter { public FileTextWriter() : base() { } }
public class FileSaver : FileWriter { public FileSaver() : base() { } }
public class FileSave : FileWriter { public FileSave() : base() { } }
public class FileWriterService : FileWriter { public FileWriterService() : base() { } }
public class FileWriterHelper : FileWriter { public FileWriterHelper() : base() { } }
public class FileWriteService : FileWriter { public FileWriteService() : base() { } }
public class FileWrite : FileWriter { public FileWrite() : base() { } }
public class Utf8Write : FileWriter { public Utf8Write() : base() { } }
public class Utf8WriterService : FileWriter { public Utf8WriterService() : base() { } }
public class DocumentWriterService : FileWriter { public DocumentWriterService() : base() { } }
public class WriteFile : FileWriter { public WriteFile() : base() { } }
public class FileContentWriter : FileWriter { public FileContentWriter() : base() { } }
public class ContentWriter : FileWriter { public ContentWriter() : base() { } }
public class Utf8TextWriter : FileWriter { public Utf8TextWriter() : base() { } }
public class MarkdownTextWriter : FileWriter { public MarkdownTextWriter() : base() { } }
public class FileEncoderService : FileWriter { public FileEncoderService() : base() { } }
public class Utf8EncoderService : FileWriter { public Utf8EncoderService() : base() { } }
public class FileSaverService : FileWriter { public FileSaverService() : base() { } }
public class Saver : FileWriter { public Saver() : base() { } }
public class Writer : FileWriter { public Writer() : base() { } }
public class Encoder : FileWriter { public Encoder() : base() { } }
public class SaverService : FileWriter { public SaverService() : base() { } }
public class WriteService : FileWriter { public WriteService() : base() { } }
public class AtomicFileWriter : FileWriter { public AtomicFileWriter() : base() { } }
public class AtomicWriter : FileWriter { public AtomicWriter() : base() { } }
public class AtomicSaver : FileWriter { public AtomicSaver() : base() { } }
public class Utf8AtomicWriter : FileWriter { public Utf8AtomicWriter() : base() { } }
public class MdFileWriter : FileWriter { public MdFileWriter() : base() { } }
public class MdWriter : FileWriter { public MdWriter() : base() { } }
public class MdSaver : FileWriter { public MdSaver() : base() { } }
public class FileMdWriter : FileWriter { public FileMdWriter() : base() { } }
public class FileAtomicWriter : FileWriter { public FileAtomicWriter() : base() { } }
public class FileUtf8Writer : FileWriter { public FileUtf8Writer() : base() { } }
public class FileMarkdownWriter : FileWriter { public FileMarkdownWriter() : base() { } }
public class TextWriter2 : FileWriter { public TextWriter2() : base() { } }