poetry run watchmedo shell-command^
    --patterns="*.py;*.jinja"^
    --ignore-directories^
    --recursive^
    --verbose^
    --command="poetry run python Assets\Scripts\Runtime\Extensions\codegen_vectors.py"^
     Assets\Scripts
