import os

os.chdir(os.path.dirname(os.path.abspath(__file__)))

try:
    if os.path.exists("err.txt"):
        with open("err.txt", "rb") as f:
            data = f.read()
        decoded = data.decode("utf-16-le", errors="ignore")
        with open("out.txt", "w", encoding="utf-8") as f:
            f.write(decoded)
    else:
        with open("out.txt", "w", encoding="utf-8") as f:
            f.write("err.txt does not exist in " + os.getcwd())
except Exception as e:
    with open("out.txt", "w", encoding="utf-8") as f:
        f.write("Error: " + str(e))
