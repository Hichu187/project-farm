"""Sinh lưới ký tự cho cá thân dài theo đường cong Bezier (dùng cho cá chạch, lươn...).
Đầu ở P0, đuôi ở P3. In ra lưới để dán vào fish_grids.py rồi sửa tay."""
import math

def bez(p, t):
    u = 1 - t
    return tuple(u**3*p[0][i] + 3*u*u*t*p[1][i] + 3*u*t*t*p[2][i] + t**3*p[3][i] for i in (0, 1))

def body(ctrl, rad=lambda t: 1.6 if t < .75 else 1.6 - (t - .75) * 3.2, n=400):
    pts = [bez(ctrl, i / n) for i in range(n + 1)]
    g = [["." for _ in range(16)] for _ in range(16)]
    for y in range(16):
        for x in range(16):
            px, py = x + .5, y + .5
            i = min(range(n + 1), key=lambda k: (pts[k][0]-px)**2 + (pts[k][1]-py)**2)
            t = i / n
            cx, cy = pts[i]
            d = math.hypot(px - cx, py - cy)
            if d > rad(t) or t > .97:
                continue
            a, b = pts[max(i-1, 0)], pts[min(i+1, n)]
            tx, ty = b[0]-a[0], b[1]-a[1]
            L = math.hypot(tx, ty) or 1
            nx, ny = -ty/L, tx/L
            if nx*-1 + ny*-1 < 0:      # pháp tuyến hướng về phía sáng (trên-trái)
                nx, ny = -nx, -ny
            o = (px-cx)*nx + (py-cy)*ny
            g[y][x] = "L" if o > .55 else "H" if o > -.45 else "M" if o > -1.1 else "D"
    return g

def show(g):
    return "\n".join('"' + "".join(r) + '",' for r in g)

if __name__ == "__main__":
    V = {
        "A_vom": [(2.5, 12.5), (2.5, 1.5), (13, 1.5), (13, 12.5)],
        "B_dau_hoi": [(3, 4), (14.5, 0.5), (15, 11), (3.5, 13.5)],
        "C_chu_S": [(2.5, 3.5), (12, 0), (4, 15.5), (13.5, 12.5)],
    }
    for k, c in V.items():
        print(k); print(show(body(c))); print()
