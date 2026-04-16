import time
import copy
from sort_algorithms import get_sort_function
from array_utils import sort_rows_elements, sort_rows_by_avg


def input_matrix():
    n = int(input("Введите количество строк (n): "))
    m = int(input("Введите количество столбцов (m): "))
    print("Введите элементы построчно через пробел:")
    matrix = []
    for i in range(n):
        row = list(map(int, input(f"Строка {i + 1}: ").split()))
        if len(row) != m:
            print(f"Ошибка: нужно {m} элементов. Повторите.")
            return input_matrix()
        matrix.append(row)
    return matrix


def print_matrix(matrix, title="Матрица"):
    print(f"\n{title}:")
    for row in matrix:
        print(" ".join(f"{x:4d}" for x in row))


def main():
    print("=== Обработка двумерного массива ===")
    matrix = input_matrix()
    print("\nИсходная матрица:")
    print_matrix(matrix)

    print("\nВыберите метод сортировки строк:")
    print("1 - Пузырьком")
    print("2 - Быстрая сортировка")
    print("3 - Сортировка вставками")
    method = int(input("Ваш выбор: "))
    sort_func = get_sort_function(method)

    # Копируем матрицу для чистоты эксперимента
    work_matrix = copy.deepcopy(matrix)

    start_time = time.time()

    # 1. Сортируем элементы внутри каждой строки
    work_matrix = sort_rows_elements(work_matrix, sort_func)

    # 2. Сортируем строки по среднему арифметическому
    work_matrix = sort_rows_by_avg(work_matrix)

    end_time = time.time()
    elapsed = end_time - start_time

    print("\nРезультат после сортировки:")
    print_matrix(work_matrix, "Преобразованная матрица")
    print(f"\nВремя выполнения: {elapsed:.6f} секунд")


if __name__ == "__main__":
    main()
