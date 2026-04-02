# config.py
# Пороговые значения и настройки

# Диапазоны датчиков
TEMP_MIN = 15.0
TEMP_MAX = 35.0
HUM_MIN = 30.0
HUM_MAX = 80.0
LIGHT_MIN = 100
LIGHT_MAX = 1000

# Пороги для фильтрации (тревоги)
TEMP_ALARM_LOW = 16.0
TEMP_ALARM_HIGH = 33.0
HUM_ALARM_HIGH = 75.0

# Имя файла лога
LOG_FILE = "log.txt"

# Количество замеров для аналитики по умолчанию
DEFAULT_ANALYTICS_WINDOW = 3