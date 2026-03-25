import numpy as np
import matplotlib.pyplot as plt

def f(x, y):
    y1, y2 = y
    dy1 = y2
    dy2 = (2*x)/np.cos(x) + x**2/np.sin(x) - y2/np.cos(x) - y1/np.sin(x) + 2
    return np.array([dy1, dy2])

def rk4(f, x0, y0, h, n):
    xs = np.zeros(n+1)
    ys = np.zeros((n+1, len(y0)))
    xs[0] = x0
    ys[0] = y0
    for i in range(n):
        k1 = f(xs[i], ys[i])
        k2 = f(xs[i] + h/2, ys[i] + h*k1/2)
        k3 = f(xs[i] + h/2, ys[i] + h*k2/2)
        k4 = f(xs[i] + h, ys[i] + h*k3)
        ys[i+1] = ys[i] + h*(k1 + 2*k2 + 2*k3 + k4)/6
        xs[i+1] = xs[i] + h
    return xs, ys

def shoot_left(beta):
    x0 = 1.2
    y0 = np.array([2.0, beta])
    h = -0.01
    n = int((0.2 - 1.2) / h)
    xs, ys = rk4(f, x0, y0, h, n)
    return xs[-1], ys[-1][0]

def F(beta):
    _, y_left = shoot_left(beta)
    return y_left - 1.0

beta_guess = 0.0
beta1 = 0.0
beta2 = 1.0
f1 = F(beta1)
f2 = F(beta2)
for _ in range(10):
    if abs(f2 - f1) < 1e-12:
        break
    beta_next = beta2 - f2 * (beta2 - beta1) / (f2 - f1)
    beta1, beta2 = beta2, beta_next
    f1, f2 = f2, F(beta2)
    if abs(f2) < 1e-8:
        break
beta_opt = beta2

xs, ys = shoot_left(beta_opt)
xs = xs[::-1]
ys = ys[::-1]

print("beta =", beta_opt)
print("y(0.2) =", ys[0][0])
print("y(1.2) =", ys[-1][0])

plt.plot(xs, ys[:, 0], 'b-', label='y(x)')
plt.xlabel('x')
plt.ylabel('y')
plt.title('Решение краевой задачи (вариант 10, стрельба влево)')
plt.grid(True)
plt.legend()
plt.show()