using DocumentFormat.OpenXml.Wordprocessing;


namespace SIPROWEBPUBLICACAO.Helper
{
    public static class WordHelper
    {
        public static TableCell CreateCell(string text, bool isBold, int width, int fontSize = 16, bool alignLeft = false)
        {
            var runProperties = new RunProperties();
            if (isBold) runProperties.Append(new Bold());
            runProperties.Append(new FontSize { Val = fontSize.ToString() });
            runProperties.Append(new RunFonts { Ascii = "Arial" });

            var run = new Run(runProperties, new Text(text ?? ""));

            var paragraphProperties = new ParagraphProperties(
                new Justification { Val = alignLeft ? JustificationValues.Left : JustificationValues.Center },
                 alignLeft ? new Indentation { Left = "60" } : null,
                new SpacingBetweenLines { After = "0", Before = "0", Line = "240", LineRule = LineSpacingRuleValues.Auto }
            );


            var paragraph = new Paragraph(paragraphProperties, run);

            var cellProperties = new TableCellProperties(
                new TableCellWidth { Type = TableWidthUnitValues.Dxa, Width = width.ToString() },
                new TableCellVerticalAlignment { Val = TableVerticalAlignmentValues.Center }
            );

            var cell = new TableCell(cellProperties, paragraph);
            return cell;
        }



    }

}