# analytics_module.py
from collections import deque


class Analytics:
    """Хранит историю замеров и вычисляет средние значения."""

    def __init__(self, window_size):
        self.window_size = window_size
        self.history = deque(maxlen=window_size)

    def add_reading(self, reading):
        """Добавляет замер в историю."""
        self.history.append(reading)

    def get_averages(self):
        """Возвращает средние значения температуры, влажности, освещённости."""
        if not self.history:
            return None
        n = len(self.history)
        avg_temp = sum(r["temperature"] for r in self.history) / n
        avg_hum = sum(r["humidity"] for r in self.history) / n
        avg_light = sum(r["light"] for r in self.history) / n
        return {
            "avg_temperature": round(avg_temp, 1),
            "avg_humidity": round(avg_hum, 1),
            "avg_light": round(avg_light, 1)
        }