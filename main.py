from flask import Flask
from dateutil.parser import parse

app = Flask(__name__)

def date_invalid(date: str) -> bool:
    try:
        parse(date, dayfirst=True)
        return True
    except ValueError:
        return False


def result_wastes(date, number) -> str:
    global wastes
    wastes[date] = number
    return f'Full wastes: {wastes}'

@app.route('/add/<date>/<int:number>')
def add(date: str, number: int) -> str:
    if date_invalid(date):
        result_wastes(date, number)

    return f'Information saved: Date: {date}, Wastes: {number}'


@app.route('/calculate/<int:year>')
def calculate(year: int) -> (str, bool):
    result = 0
    if len(wastes) == 0:
        return f'Wastes emply'
    if not isinstance(year, int):
        return False
    else:
        for i_date in wastes:
            if str(year) in i_date:
                result += wastes[i_date]

    return f'For the specified {year}, you spent: {result}'

@app.route('/calculate_month/<int:year>/<int:month>')
def calculate_month(year: int, month: int) -> (str, bool):
    result = 0
    if len(wastes) == 0:
        return f'Wastes emply'
    if not isinstance(year, int) or not isinstance(month, int):
        return False
    else:
        for i_date in wastes:
            if str(year) in i_date and str(month) in i_date:
                result += wastes[i_date]
    return f'For the specified {year}-{month}, you spent: {result}'


@app.route('/info')
def info() -> str:
    return f'Result wastes: {wastes}'


wastes = dict()
if __name__ == '__main__':
    app.run(debug=True)
