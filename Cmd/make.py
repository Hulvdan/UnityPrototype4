import os
from pathlib import Path
import subprocess

import typer

app = typer.Typer()

CMD_ROOT = Path("Cmd")



@app.command()
def code(watch: bool = False):
    """Run all code generation scripts."""
    if os.name != "nt":
        raise ValueError(f"os '{os.name}' is to be implemented!")

    if watch:
        executable = CMD_ROOT / "codegen-watch.bat"
    else:
        executable = CMD_ROOT / "codegen.bat"

    subprocess.run([executable], shell=True, check=True)


@app.command()
def doc():
    """Generate docs."""
    if os.name != "nt":
        raise ValueError(f"os '{os.name}' is to be implemented!")

    executable = CMD_ROOT / "doc.bat"
    subprocess.run([executable], shell=True, check=True)


if __name__ == "__main__":
    app()

