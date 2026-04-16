import unittest
import sys
import os

# Добавляем путь к модулям проекта
sys.path.append(os.path.dirname(os.path.abspath(__file__)))

from sort_algorithms import bubble_sort, quick_sort, insertion_sort
from array_utils import row_average, sort_rows_by_avg, sort_rows_elements


class TestSortAlgorithms(unittest.TestCase):
    """Тестирование алгоритмов сортировки"""

    def test_bubble_sort(self):
        arr = [3, 1, 4, 1, 5, 9, 2]
        expected = sorted(arr)
        self.assertEqual(bubble_sort(arr), expected)

        arr2 = []
        self.assertEqual(bubble_sort(arr2), [])

        arr3 = [5]
        self.assertEqual(bubble_sort(arr3), [5])

        arr4 = [2, 1]
        self.assertEqual(bubble_sort(arr4), [1, 2])

    def test_quick_sort(self):
        arr = [3, 1, 4, 1, 5, 9, 2]
        expected = sorted(arr)
        self.assertEqual(quick_sort(arr), expected)

        arr2 = []
        self.assertEqual(quick_sort(arr2), [])

        arr3 = [5]
        self.assertEqual(quick_sort(arr3), [5])

    def test_insertion_sort(self):
        arr = [3, 1, 4, 1, 5, 9, 2]
        expected = sorted(arr)
        self.assertEqual(insertion_sort(arr), expected)

        arr2 = []
        self.assertEqual(insertion_sort(arr2), [])

        arr3 = [5]
        self.assertEqual(insertion_sort(arr3), [5])

    def test_all_sorts_consistency(self):
        """Все три сортировки должны давать одинаковый результат"""
        test_cases = [
            [3, 1, 4, 1, 5, 9, 2],
            [10, 9, 8, 7, 6],
            [1, 2, 3, 4, 5],
            [5, 4, 3, 2, 1],
            [0, 0, 0, 0],
            [-5, -1, -3, -2, -4]
        ]
        for arr in test_cases:
            with self.subTest(arr=arr):
                expected = sorted(arr)
                self.assertEqual(bubble_sort(arr.copy()), expected)
                self.assertEqual(quick_sort(arr.copy()), expected)
                self.assertEqual(insertion_sort(arr.copy()), expected)


class TestArrayUtils(unittest.TestCase):
    """Тестирование утилит для работы с массивами"""

    def test_row_average(self):
        self.assertAlmostEqual(row_average([1, 2, 3]), 2.0)
        self.assertAlmostEqual(row_average([-5, 0, 5]), 0.0)
        self.assertAlmostEqual(row_average([10]), 10.0)
        self.assertAlmostEqual(row_average([]), 0.0)

    def test_sort_rows_by_avg(self):
        matrix = [
            [5, 2, 8],  # среднее = 5.0
            [3, 1, 4]  # среднее = 2.666...
        ]
        expected = [
            [3, 1, 4],
            [5, 2, 8]
        ]
        result = sort_rows_by_avg(matrix)
        self.assertEqual(result, expected)

    def test_sort_rows_elements(self):
        matrix = [
            [5, 2, 8],
            [3, 1, 4]
        ]
        expected = [
            [2, 5, 8],
            [1, 3, 4]
        ]
        # Используем быструю сортировку
        from sort_algorithms import quick_sort
        result = sort_rows_elements(matrix, quick_sort)
        self.assertEqual(result, expected)

    def test_full_pipeline(self):
        """Полный цикл: сортировка строк + сортировка по среднему"""
        from sort_algorithms import quick_sort
        matrix = [
            [3, 2, 1],  # после сортировки строк [1,2,3], среднее 2
            [6, 5, 4],  # после [4,5,6], среднее 5
            [0, -1, -2]  # после [-2,-1,0], среднее -1
        ]
        # Сортируем элементы внутри строк
        sorted_rows = sort_rows_elements(matrix, quick_sort)
        # Сортируем строки по среднему
        final = sort_rows_by_avg(sorted_rows)
        # Ожидаемый порядок: строка со средним -1 (индекс 2), затем 2, затем 5
        expected = [
            [-2, -1, 0],
            [1, 2, 3],
            [4, 5, 6]
        ]
        self.assertEqual(final, expected)


if __name__ == '__main__':
    unittest.main()
