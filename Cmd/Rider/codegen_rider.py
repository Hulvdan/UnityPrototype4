"""
A QoL tool that generates "Directory.Build.props" with the listed files
(like .gitignore, README.md, etc.) so that the fuken Rider indexes them.

ATTENTION:
It only supports including all the files in the root directory.

RELATED ISSUE: "readme.md and Documents~ directory in root of Unity project are not indexed"
https://rider-support.jetbrains.com/hc/en-us/community/posts/12936423188882-readme-md-and-Documents-directory-in-root-of-Unity-project-are-not-indexed
"""
import fnmatch
import glob
import os
from pathlib import Path

from jinja2 import StrictUndefined, Template

PROJECT_ROOT = Path(__file__).parent.parent.parent

SCRIPT_PATH = Path(__file__).relative_to(Path.cwd()).as_posix()
OUT_FILE_PATH = PROJECT_ROOT / "Directory.Build.props"
TEMPLATE_PATH = "Directory.Build.props.j2"


def get_root_files() -> list[str]:
    res = []

    # Copy-Paste from .vscode/settings.json
    exclude_patterns = {
        ".idea": True,
        ".venv": True,
        "Library": True,
        "obj": True,
        "Packages": True,
        "ProjectSettings": True,
        "Temp": True,
        "UserSettings": True,
        "*.DotSettings.user": True,
        "*.DotSettings": True,
        "*.sln": True,
        "**/.DS_Store": True,
        "**/.git": True,
        "**/*.csproj": True,
        "**/Thumbs.db": True,
        "**/*.meta": True,
    }

    for local_filepath in glob.glob(".*") + glob.glob("*"):
        if os.path.isdir(local_filepath):
            continue

        should_be_excluded = False
        filepath = str(PROJECT_ROOT / local_filepath)
        for exclude_pattern in exclude_patterns:
            if fnmatch.fnmatch(filepath, exclude_pattern):
                should_be_excluded = True
                break

        if should_be_excluded:
            continue

        res.append(local_filepath)

    return res


def main() -> None:
    template_path = Path(__file__).parent / TEMPLATE_PATH
    with open(template_path) as in_file:
        template = Template(in_file.read(), undefined=StrictUndefined)

    data = template.render(
        generated_via=SCRIPT_PATH,
        root_files=get_root_files(),
    )

    with open(OUT_FILE_PATH, "w") as out_file:
        out_file.write(data)


if __name__ == "__main__":
    main()
