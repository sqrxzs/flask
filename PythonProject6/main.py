import numpy as np
import matplotlib.pyplot as plt


print("=" * 60)
print("Задание 1: СМО с отказами")
print("=" * 60)

lambda_val = 16
t_service_min = 10
t_service_hour = t_service_min / 60
mu = 1 / t_service_hour
alpha = lambda_val / mu

n = 2
print(f"Исходные данные: λ = {lambda_val} заявок/ч, среднее время обслуживания = {t_service_min} мин → μ = {mu:.2f} заявок/ч")
print(f"Нагрузка α = λ/μ = {alpha:.3f}")

def erlang_calc(n, alpha):
    # p0
    sum_p0 = 0
    for k in range(n+1):
        sum_p0 += alpha**k / np.math.factorial(k)
    p0 = 1 / sum_p0
    p_otk = p0 * (alpha**n / np.math.factorial(n))
    return p0, p_otk

p0, p_otk = erlang_calc(n, alpha)
p_obs = 1 - p_otk
A = lambda_val * p_obs
M = A / mu

print("\nХарактеристики при 2 бухгалтерах:")
print(f"  Вероятность простоя каналов P0 = {p0:.4f}")
print(f"  Вероятность отказа Pотк = {p_otk:.4f}")
print(f"  Вероятность обслуживания Pобс = {p_obs:.4f}")
print(f"  Абсолютная пропускная способность A = {A:.2f} заявок/ч")
print(f"  Среднее число занятых бухгалтеров M = {M:.2f}")

n_values = range(1, 8)
p_otk_list = []
p_obs_list = []
for n_cur in n_values:
    _, p_otk_cur = erlang_calc(n_cur, alpha)
    p_otk_list.append(p_otk_cur)
    p_obs_list.append(1 - p_otk_cur)

print("\nПодбор числа бухгалтеров:")
print("  n   Pотк       Pобс")
for n_cur, p_otk_cur in zip(n_values, p_otk_list):
    print(f" {n_cur:2d}   {p_otk_cur:.4f}   {1-p_otk_cur:.4f}")

for n_cur, p_obs_cur in zip(n_values, p_obs_list):
    if p_obs_cur > 0.85:
        print(f"\nМинимальное число бухгалтеров для Pобс > 85%: n = {n_cur}")
        break

# График
plt.figure(figsize=(8, 5))
plt.plot(n_values, p_obs_list, 'bo-', label='Pобс')
plt.axhline(y=0.85, color='r', linestyle='--', label='85%')
plt.xlabel('Число бухгалтеров n')
plt.ylabel('Вероятность обслуживания')
plt.title('Зависимость вероятности обслуживания от числа бухгалтеров')
plt.grid(True)
plt.legend()
plt.show()


print("\n" + "=" * 60)
print("Задание 2: Имитационное моделирование")
print("=" * 60)

lambda_pois = 1.8
mu_exp = 0.5

u = np.random.uniform(0, 1, 15)
x = -np.log(1 - u) / mu_exp


y = np.random.poisson(lambda_pois, 15)

print("\nСмоделированные значения:")
print("  №    X (длительность обслуживания, ед.вр.)    Y (число клиентов)")
for i in range(15):
    print(f"{i+1:3d}   {x[i]:.4f}                              {y[i]:2d}")

print("\nИнтерпретация:")
print("  - Значения X означают время обслуживания одного клиента (в тех же единицах времени, что и интенсивность).")
print(f"    Среднее теоретическое время обслуживания = 1/μ = {1/mu_exp:.2f}.")
print(f"    В выборке среднее = {np.mean(x):.2f}.")
print("  - Значения Y означают число клиентов, поступивших за единицу времени (например, за час).")
print(f"    Среднее теоретическое = λ = {lambda_pois:.1f}. В выборке среднее = {np.mean(y):.2f}.")
print("  - Эти числа можно использовать для имитационного моделирования работы парикмахерской:",
      "для каждого часа генерируется число клиентов, и для каждого клиента — время обслуживания.")