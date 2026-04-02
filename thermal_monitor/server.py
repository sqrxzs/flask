# server.py
from flask import Flask, request, jsonify
from functools import wraps
import config
from data_acquisition import DataAcquisition
from filter_module import Filter
from logger_module import Logger
from analytics_module import Analytics

app = Flask(__name__)

# Глобальное состояние для хранения последнего измерения и истории аналитики
last_reading = None
analytics = Analytics(config.DEFAULT_ANALYTICS_WINDOW)  # окно по умолчанию

# Пороги (можно менять через API)
current_thresholds = {
    "temp_max": config.TEMP_ALARM_HIGH,
    "temp_min": config.TEMP_ALARM_LOW,
    "hum_max": config.HUM_ALARM_HIGH
}


# --- Аутентификация (Basic Auth) ---
def check_auth(username, password):
    return username == "admin" and password == "lab2025"


def authenticate():
    return jsonify({"error": "Unauthorized"}), 401


def requires_auth(f):
    @wraps(f)
    def decorated(*args, **kwargs):
        auth = request.authorization
        if not auth or not check_auth(auth.username, auth.password):
            return authenticate()
        return f(*args, **kwargs)

    return decorated


# --- Эндпоинты API ---

@app.route('/api/last', methods=['GET'])
def get_last():
    """Вернуть последнее измерение."""
    if last_reading is None:
        return jsonify({"error": "No measurements yet"}), 404
    return jsonify(last_reading)


@app.route('/api/stats', methods=['GET'])
def get_stats():
    """Вернуть средние значения за последние N замеров. N передаётся параметром ?n=5."""
    n = request.args.get('n', default=config.DEFAULT_ANALYTICS_WINDOW, type=int)
    if n <= 0:
        return jsonify({"error": "N must be positive"}), 400
    # Создаём временный объект Analytics с нужным окном
    temp_analytics = Analytics(n)
    # К сожалению, у нас нет доступа ко всей истории из analytics.history,
    # поэтому мы должны были хранить полную историю. Упростим: будем хранить список всех замеров.
    # Для демонстрации создадим глобальный список.
    if not all_readings:
        return jsonify({"error": "No data available"}), 404
    # Берём последние n записей
    recent = all_readings[-n:]
    if not recent:
        return jsonify({"error": "Not enough data"}), 404
    avg_temp = sum(r["temperature"] for r in recent) / len(recent)
    avg_hum = sum(r["humidity"] for r in recent) / len(recent)
    avg_light = sum(r["light"] for r in recent) / len(recent)
    return jsonify({
        "n": len(recent),
        "avg_temperature": round(avg_temp, 1),
        "avg_humidity": round(avg_hum, 1),
        "avg_light": round(avg_light, 1)
    })


@app.route('/api/threshold', methods=['POST'])
@requires_auth
def set_threshold():
    """Изменить пороги. Тело: {"temp_max": 30, "temp_min": 15, "hum_max": 80}."""
    data = request.get_json()
    if not data:
        return jsonify({"error": "JSON body required"}), 400
    if "temp_max" in data:
        current_thresholds["temp_max"] = data["temp_max"]
        config.TEMP_ALARM_HIGH = data["temp_max"]  # обновляем глобальный конфиг
    if "temp_min" in data:
        current_thresholds["temp_min"] = data["temp_min"]
        config.TEMP_ALARM_LOW = data["temp_min"]
    if "hum_max" in data:
        current_thresholds["hum_max"] = data["hum_max"]
        config.HUM_ALARM_HIGH = data["hum_max"]
    return jsonify({"message": "Thresholds updated", "current": current_thresholds})


@app.route('/api/log', methods=['GET'])
@requires_auth
def get_log():
    """Вернуть последние 10 строк из log.txt."""
    try:
        with open(config.LOG_FILE, "r", encoding="utf-8") as f:
            lines = f.readlines()
        last_lines = lines[-10:] if len(lines) >= 10 else lines
        return jsonify({"log": [line.strip() for line in last_lines]})
    except FileNotFoundError:
        return jsonify({"log": []})


@app.route('/api/collect', methods=['POST'])
def collect_one():
    """Выполнить один цикл сбора, фильтрации, логирования и обновить последнее значение."""
    global last_reading, all_readings
    reading = DataAcquisition.get_reading()
    last_reading = reading

    # Используем текущие пороги из config (могли быть изменены через API)
    status, message = Filter.check(reading)
    Logger.log(reading, status, message)

    # Сохраняем в глобальную историю для статистики
    if 'all_readings' not in globals():
        all_readings = []
    all_readings.append(reading)

    # Обновляем аналитику (для простоты используем глобальный объект analytics с фиксированным окном)
    analytics.add_reading(reading)

    return jsonify({
        "reading": reading,
        "status": status,
        "message": message
    })


# Глобальный список всех замеров (для stats)
all_readings = []

# Запуск сервера только на локальном хосте, порт 8080
if __name__ == '__main__':
    app.run(host='127.0.0.1', port=8080, debug=False)