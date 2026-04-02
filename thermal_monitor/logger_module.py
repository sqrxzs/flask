# logger_module.py
import datetime
import config


class Logger:
    """Сохраняет события в файл с меткой времени."""

    @staticmethod
    def log(reading, status, message):
        """Записывает строку в лог-файл."""
        timestamp = datetime.datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        log_line = (f"[{timestamp}] T={reading['temperature']}°C, "
                    f"H={reading['humidity']}%, L={reading['light']} лк -> "
                    f"{status}: {message}\n")
        try:
            with open(config.LOG_FILE, "a", encoding="utf-8") as f:
                f.write(log_line)
        except IOError as e:
            print(f"Ошибка записи в лог: {e}")