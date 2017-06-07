import csv

with open("catalogue_editor.csv", newline="", encoding="utf8") as f:
	reader = csv.reader(f, delimiter=',', quoting=csv.QUOTE_ALL)
	next(reader)
	catalogue = [row[0:5] for row in reader]

with open("Catalogue.cs", "w", encoding="utf8") as file:
	file.write("""namespace FreeRaider.Loader
{
	public class Catalogue
	{
		public static int[][] Models =
		{
""")
	file.write(",\n".join(["\t\t\tnew [] { " + ", ".join(r) + " }" for r in catalogue]))
	file.write("""
		};
	}
}
""")