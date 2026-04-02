# data_acquisition.py
import random
import config


class DataAcquisition:
    """Генератор случайных показаний датчиков."""

    @staticmethod
    def get_reading():
        """Возвращает словарь с показаниями температуры, влажности, освещённости."""
        return {
            "temperature": round(random.uniform(config.TEMP_MIN, config.TEMP_MAX), 1),
            "humidity": round(random.uniform(config.HUM_MIN, config.HUM_MAX), 1),
            "light": random.randint(config.LIGHT_MIN, config.LIGHT_MAX)
        }