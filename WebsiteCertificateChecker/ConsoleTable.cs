using System.CommandLine.Invocation;

namespace ConsoleTable
{
    public class Table
    {
        private List<Row> rows = [];

        public string Separator { get; set; } = "  ";
        public char FillChar { get; set; } = ' ';

        public Dictionary<int, int> MinColumnSizes { get; set; } = [];

        public void AddRow(Row row)
        {
            rows.Add(row);
        }

        public void Write()
        {
            var columnSizes = GetColumnSizes();

            foreach (var row in rows)
            {
                for (int i = 0; i < row.cells.Count; i++)
                {
                    var cell = row.cells[i];

                    var fillCount = columnSizes[i] - cell.Text.Length;

                    if (cell.Alignment == TextAlignment.Right)
                    {
                        Console.Write(new string(FillChar, fillCount));
                    }
                    if (cell.Alignment == TextAlignment.Center)
                    {
                        Console.Write(new string(FillChar, fillCount / 2));
                    }

                    cell.Text.Write();

                    if (cell.Alignment == TextAlignment.Left)
                    {
                        Console.Write(new string(FillChar, fillCount));
                    }
                    if (cell.Alignment == TextAlignment.Center)
                    {
                        Console.Write(new string(FillChar, fillCount - (fillCount / 2)));
                    }

                    if (i < row.cells.Count - 1)
                    {
                        Console.Write(Separator);
                    }
                }

                Console.WriteLine();
            }
        }

        private List<int> GetColumnSizes()
        {
            var sizes = new List<int>();

            foreach (var row in rows)
            {
                for (int i = 0; i < row.cells.Count; i++)
                {
                    var length = row.cells[i].Text.Length;

                    if (i == sizes.Count)
                    {
                        sizes.Add(length);
                        continue;
                    }

                    if (length > sizes[i])
                    {
                        sizes[i] = length;
                    }
                }
            }

            for (int i = 0; i < sizes.Count; i++)
            {
                if (!MinColumnSizes.TryGetValue(i, out int minSize)) continue;

                if (sizes[i] < minSize)
                {
                    sizes[i] = minSize;
                }
            }

            return sizes;
        }
    }

    public class Row
    {
        internal List<Cell> cells = [];

        public void AddCell(Cell cell)
        {
            cells.Add(cell);
        }

        public void AddCell(Text text, TextAlignment alignment = TextAlignment.Left)
        {
            AddCell(new(text, alignment));
        }

        public void AddEmpty()
        {
            AddCell(string.Empty);
        }
    }

    public readonly record struct Cell(Text Text, TextAlignment Alignment);

    public enum TextAlignment
    {
        Left,
        Right,
        Center,
    }

    public class Text
    {
        private List<TextFragment> fragments;
        public int Length { get; private set; }

        private Text(int count, int length)
        {
            fragments = new(count);
            Length = length;
        }

        public Text(string value, ConsoleColor? color)
        {
            fragments = [new(value, color)];
            Length = value.Length;
        }

        public static implicit operator Text(string value) => new(value, null);

        public static Text operator +(Text left, Text right)
        {
            var count = left.fragments.Count + right.fragments.Count;
            var length = left.Length + right.Length;

            var newText = new Text(count, length);

            newText.fragments.AddRange(left.fragments);
            newText.fragments.AddRange(right.fragments);

            return newText;
        }

        internal void Write()
        {
            foreach (var frag in fragments)
            {
                if (!frag.Color.HasValue)
                {
                    Console.Write(frag.Value);
                    continue;
                }

                Console.ForegroundColor = frag.Color.Value;
                Console.Write(frag.Value);
                Console.ResetColor();
            }
        }
    }

    internal readonly record struct TextFragment(string Value, ConsoleColor? Color);
}
