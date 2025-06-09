from docling.document_converter import DocumentConverter, FormatOption
from docling.datamodel.pipeline_options import PdfPipelineOptions, TableFormerMode, EasyOcrOptions
from docling.datamodel.base_models import InputFormat
from docling.pipeline.standard_pdf_pipeline import StandardPdfPipeline
from docling.backend.docling_parse_v2_backend import DoclingParseV2DocumentBackend
        
pipeline_options = PdfPipelineOptions()
pipeline_options.do_ocr = True
ocr_options = EasyOcrOptions(lang=['en'], force_full_page_ocr=False)
pipeline_options.ocr_options = ocr_options
pipeline_options.do_table_structure = True
pipeline_options.table_structure_options.do_cell_matching = True
pipeline_options.table_structure_options.mode = TableFormerMode.ACCURATE
        
format_options = {
    InputFormat.PDF: FormatOption(
        pipeline_cls=StandardPdfPipeline,
        pipeline_options=pipeline_options,
        backend=DoclingParseV2DocumentBackend
    )
}
        
converter = DocumentConverter(format_options=format_options)
source = "./some_testing.pdf"
result = converter.convert(source)
import json
json_data = result.document.export_to_dict()
with open("output.json", "w", encoding="utf-8") as f:
    json.dump(json_data, f, ensure_ascii=False, indent=2)

