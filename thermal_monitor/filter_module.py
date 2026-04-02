# filter_module.py
import config


class Filter:
    """Анализирует показания и возвращает статус и сообщение."""

    @staticmethod
    def check(reading):
        """
        Принимает словарь с ключами temperature, humidity, light.
        Возвращает кортеж (status, message), где status: 'OK', 'ALARM', 'WATER'.
        """
        temp = reading["temperature"]
        hum = reading["humidity"]

        if temp > config.TEMP_ALARM_HIGH:
            return "ALARM", f"высокая температура ({temp}°C)"
        elif temp < config.TEMP_ALARM_LOW:
            return "ALARM", f"низкая температура ({temp}°C)"
        elif hum > config.HUM_ALARM_HIGH:
            return "WATER", f"требуется полив (влажность {hum}%)"
        else:
            return "OK", "в пределах нормы"