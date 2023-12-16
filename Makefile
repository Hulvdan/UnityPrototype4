.DEFAULT_GOAL := all

all:
	poetry run python Cmd/make.py --help

code:
	poetry run python Cmd/make.py code

code-watch:
	poetry run python Cmd/make.py code --watch

doc:
	poetry run python Cmd/make.py doc
