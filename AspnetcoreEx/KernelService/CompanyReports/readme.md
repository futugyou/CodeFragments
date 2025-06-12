# Analysis of Pipeline class method call sequence 

- [RAG-Challenge](https://github.com/IlyaRice/RAG-Challenge-2)

## 1. Initialization

- `__init__`: Initialize path, configuration, and convert subset.json to subset.csv if needed.

## 2. Download Docling model (optional)

- This step will download some relatively large LLM models,
and my local PC does not have enough space to complete this task.

- `download_docling_models`: Download Docling related models on Huggingface in advance.

## 3. Parse PDF report

- C# does not have a powerful PDF library like `Docling`,
so it is recommended to use `Docling` to process the `PDF` data(`docling_example.py`),
and then C# to handle (`PDFParser.Docling.cs`) the rest of the process.

- C# alse can use `UglyToad.PdfPig` to handle PDF.

- `parse_pdf_reports`: Parse PDF report (optional concurrent/serial).

- `parse_pdf_reports_parallel` or `parse_pdf_reports_sequential`

## 4. Serialize table

- `serialize_tables`: Parallel processing of tables in parsed reports.

## 5. Merge report

- `merge_reports`: Merge complex JSON reports into simple structures.

## 6. Export reports to Markdown

- `export_reports_to_markdown`: Export the processed reports to Markdown format.

## 7. Split reports

- `chunk_reports`: Split reports into smaller chunks.

## 8. Create vector databases

- `create_vector_dbs`: Create vector databases based on the split reports.

## 9. Create BM25 databases

- `create_bm25_db`: Create BM25 databases based on the split reports.

## 10. Process questions

- `process_questions`: Process all questions and output answers.

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
