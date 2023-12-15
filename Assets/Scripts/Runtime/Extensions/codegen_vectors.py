from jinja2 import Template, StrictUndefined
from pathlib import Path


FILENAME_TEMPLATE = "{}Extensions.cs"
TEMPLATE_PATH = "vectors.jinja"

DATA = [
    ("Vector2", ("x", "y"), "float"),
    ("Vector2Int", ("x", "y"), "int"),
    ("Vector3", ("x", "y", "z"), "float"),
    ("Vector3Int", ("x", "y", "z"), "int"),
]


def main():
    template_path = Path(__file__).parent / TEMPLATE_PATH
    with open(template_path) as in_file:
        template = Template(in_file.read(), undefined=StrictUndefined)

    for classname, coords, vartype in DATA:
        data = template.render(classname=classname, coords=coords, vartype=vartype)

        out_file = Path(__file__).parent / FILENAME_TEMPLATE.format(classname)
        with open(out_file, "w") as out_file:
            out_file.write(data)


if __name__ == "__main__":
    main()
