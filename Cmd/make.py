import os
import subprocess
from pathlib import Path

import typer

app = typer.Typer()

CMD_ROOT = Path("Cmd")


@app.command()
def code(watch: bool = False):
    """Run all code generation scripts."""
    if os.name != "nt":
        raise ValueError(f"os '{os.name}' is yet to be implemented!")

    if watch:
        executable = CMD_ROOT / "codegen-watch.bat"
    else:
        executable = CMD_ROOT / "codegen.bat"

    subprocess.run([executable], shell=True, check=True)


@app.command()
def doc():
    """Generate docs."""
    if os.name != "nt":
        raise ValueError(f"os '{os.name}' is yet to be implemented!")

    subprocess.run(
        ["doxygen", Path("Docs") / "doxygen" / "Doxyfile"],
        shell=True,
        check=True,
    )


@app.command()
def rider():
    """Make the fuken Rider be aware of useful files in the root directory."""
    if os.name != "nt":
        raise ValueError(f"os '{os.name}' is yet to be implemented!")

    subprocess.run(
        ["poetry", "run", "python", "Cmd/Rider/codegen_rider.py"],
        shell=True,
        check=True,
    )


if __name__ == "__main__":
    app()
