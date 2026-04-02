# main.py
import time
from data_acquisition import DataAcquisition
from filter_module import Filter
from logger_module import Logger
from analytics_module import Analytics
import config


def main():
    print("=== Мониторинг теплицы (консольная версия) ===")
    N = int(input(f"Введите окно аналитики (N замеров, по умолч. {config.DEFAULT_ANALYTICS_WINDOW}): ")
            or config.DEFAULT_ANALYTICS_WINDOW)

    analytics = Analytics(N)
    num_iterations = 10

    for i in range(1, num_iterations + 1):
        # 1. Сбор данных
        reading = DataAcquisition.get_reading()

        # 2. Фильтрация
        status, message = Filter.check(reading)

        # 3. Логирование
        Logger.log(reading, status, message)

        # 4. Вывод в консоль
        print(
            f"[Замер {i}] T={reading['temperature']}°C, H={reading['humidity']}%, Light={reading['light']} лк -> {status}: {message}")

        # 5. Добавление в аналитику
        analytics.add_reading(reading)

        # 6. Каждые N замеров показываем аналитику
        if i % N == 0:
            avg = analytics.get_averages()
            if avg:
                print(f"--- Аналитика за последние {N} замеров: "
                      f"средняя T={avg['avg_temperature']}°C, "
                      f"средняя H={avg['avg_humidity']}%, "
                      f"средний свет={avg['avg_light']} лк ---")
        time.sleep(0.5)  # небольшая пауза для наглядности


if __name__ == "__main__":
    main()