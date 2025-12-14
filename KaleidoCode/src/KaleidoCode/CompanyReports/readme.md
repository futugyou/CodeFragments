# Analysis of Pipeline class method call sequence 

- [RAG-Challenge](https://github.com/IlyaRice/RAG-Challenge-2)

## 1. Initialization

- Initialize path, configuration, and convert subset.json to subset.csv if needed.

## 2. Download Docling model (optional)

- This step will download some relatively large LLM models,
and my local PC does not have enough space to complete this task.

- Download Docling related models on Huggingface in advance.

```py
pip install docling
python docling_example.py
```

## 3. Parse PDF report

- This operation will generate `PdfReport` array, and write it in file.

- C# does not have a powerful PDF library like `Docling`,
so it is recommended to use `Docling` to process the `PDF` data(`like docling_example.py`),
and then C# to handle (`PDFParser.Docling.cs`) the rest of the process.

- C# alse can use `UglyToad.PdfPig` to handle PDF (`PDFParser.PdfPig.cs`).

- `ParseAndExportAsync`: Parse PDF report (optional concurrent/serial).

- `ParseAndExportParallelAsync`

## 4. Serialize table  (`TableSerializer.cs`)

- This operation fills the `Serialized` field for `ReportTable` object of `PdfReport`

- `ProcessDirectoryParallelAsync`: Parallel processing of tables in parsed reports.

## 5. Merge report (`PageTextPreparation.cs`)

- This operation will generate `ProcessedReport` array, and write it in file.

- `ProcessReportsAsync`: Merge complex JSON reports into simple structures.

## 6. Export reports to Markdown (`PageTextPreparation.cs`)

- `ExportToMarkdownAsync`: Export the processed reports to Markdown format.

## 7. Split reports (`TextSplitter.cs`)

- This operation fills the `Chunks` field for `ProcessedReportContent` object of `ProcessedReport`

- `SplitAllReportsAsync`: Split reports into smaller chunks.

## 8. Create vector databases (`Ingestor.VectorDB.cs`)

- `ProcessReportsAsync`: Create vector databases based on the split reports.

## 9. Create BM25 databases (`Ingestor.BM25.cs`)

- `ProcessReportsAsync`: Create BM25 databases based on the split reports.

## 10. Process questions (`QuestionsProcessor.cs`)

- `ProcessAllQuestionsAsync`: Process all questions and output answers.

---

## Typical pipeline calling sequence

```text
__init__
↓
download_docling_models (optional)
↓
parse_pdf_reports
↓
serialize_tables
↓
merge_reports
↓
export_reports_to_markdown
↓
chunk_reports
↓
create_vector_dbs
↓
create_bm25_db (optional)
↓
process_questions
```
