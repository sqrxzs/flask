import subprocess
import os
from os import abort

from flask import Flask, request, abort, send_from_directory

app = Flask(__name__)

def save_file(result) -> None:
    with open('test.txt', 'w') as file:
        file.write(result.stdout)


def read_file() -> str:
    with open('test.txt', 'r') as file:
        file_string = file.read()
    return file_string


@app.route("/ps/", methods=["GET"])
def ps():

    args: list[str] = request.args.getlist('args')
    result = subprocess.run(['ps', args[0]], capture_output=True, text=True)
    save_file(result=result)

    return f'<p>Что бы скачать файл напишите: <p>(/download_file)</p></p>Данные вашего запроса: <p>{read_file()}</p>'

@app.route("/download_file/", methods=["GET"])
def download_file():

    file_path = 'test.txt'

    if not os.path.exists(file_path):
        abort(404, description='File is not')

    return send_from_directory(
        directory=os.path.dirname(os.path.abspath(file_path)),
        path=os.path.basename(file_path),
        as_attachment=True,
        download_name='system_test.txt'
    )

if __name__ == '__main__':
    # app.config["WTF_CSRF_ENABLED"] = False
    app.run(debug=True)