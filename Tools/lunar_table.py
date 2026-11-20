"""Sinh bảng âm lịch Việt Nam (thuật toán Hồ Ngọc Đức, múi giờ +7) cho Về Quê – Đợt 2 Phần 2.

Chạy: python Tools/lunar_table.py Assets/_Game/Data/Calendar/lunar_2026_2040.json
Mỗi tháng âm: s = ngày dương bắt đầu (DayKey: số ngày kể từ 01/01/2026), d = số ngày (29/30),
m = tháng (1–12), l = 1 nếu tháng nhuận, y = năm âm. Lúc chơi chỉ tra bảng (LunarTable), không tính thiên văn.
"""
import math, json, sys

PI = math.pi
TZ = 7.0


def INT(x):
    return math.floor(x)


def jd_from_date(dd, mm, yy):
    a = (14 - mm) // 12
    y = yy + 4800 - a
    m = mm + 12 * a - 3
    jd = dd + (153 * m + 2) // 5 + 365 * y + y // 4 - y // 100 + y // 400 - 32045
    if jd < 2299161:
        jd = dd + (153 * m + 2) // 5 + 365 * y + y // 4 - 32083
    return jd


def date_from_jd(jd):
    a = jd + 32044
    b = (4 * a + 3) // 146097
    c = a - (b * 146097) // 4
    d = (4 * c + 3) // 1461
    e = c - (1461 * d) // 4
    m = (5 * e + 2) // 153
    day = e - (153 * m + 2) // 5 + 1
    month = m + 3 - 12 * (m // 10)
    year = b * 100 + d - 4800 + m // 10
    return day, month, year


def new_moon(k):
    T = k / 1236.85
    T2 = T * T
    T3 = T2 * T
    dr = PI / 180
    Jd1 = 2415020.75933 + 29.53058868 * k + 0.0001178 * T2 - 0.000000155 * T3
    Jd1 = Jd1 + 0.00033 * math.sin((166.56 + 132.87 * T - 0.009173 * T2) * dr)
    M = 359.2242 + 29.10535608 * k - 0.0000333 * T2 - 0.00000347 * T3
    Mpr = 306.0253 + 385.81691806 * k + 0.0107306 * T2 + 0.00001236 * T3
    F = 21.2964 + 390.67050646 * k - 0.0016528 * T2 - 0.00000239 * T3
    C1 = (0.1734 - 0.000393 * T) * math.sin(M * dr) + 0.0021 * math.sin(2 * dr * M)
    C1 = C1 - 0.4068 * math.sin(Mpr * dr) + 0.0161 * math.sin(dr * 2 * Mpr)
    C1 = C1 - 0.0004 * math.sin(dr * 3 * Mpr)
    C1 = C1 + 0.0104 * math.sin(dr * 2 * F) - 0.0051 * math.sin(dr * (M + Mpr))
    C1 = C1 - 0.0074 * math.sin(dr * (M - Mpr)) + 0.0004 * math.sin(dr * (2 * F + M))
    C1 = C1 - 0.0004 * math.sin(dr * (2 * F - M)) - 0.0006 * math.sin(dr * (2 * F + Mpr))
    C1 = C1 + 0.0010 * math.sin(dr * (2 * F - Mpr)) + 0.0005 * math.sin(dr * (2 * Mpr + M))
    if T < -11:
        deltat = 0.001 + 0.000839 * T + 0.0002261 * T2 - 0.00000845 * T3 - 0.000000081 * T * T3
    else:
        deltat = -0.000278 + 0.000265 * T + 0.000262 * T2
    return Jd1 + C1 - deltat


def sun_longitude(jdn):
    T = (jdn - 2451545.0) / 36525
    T2 = T * T
    dr = PI / 180
    M = 357.52910 + 35999.05030 * T - 0.0001559 * T2 - 0.00000048 * T * T2
    L0 = 280.46645 + 36000.76983 * T + 0.0003032 * T2
    DL = (1.914600 - 0.004817 * T - 0.000014 * T2) * math.sin(dr * M)
    DL = DL + (0.019993 - 0.000101 * T) * math.sin(dr * 2 * M) + 0.000290 * math.sin(dr * 3 * M)
    L = (L0 + DL) * dr
    return L - PI * 2 * INT(L / (PI * 2))


def get_new_moon_day(k):
    return INT(new_moon(k) + 0.5 + TZ / 24)


def get_sun_longitude(day_number):
    return INT(sun_longitude(day_number - 0.5 - TZ / 24) / PI * 6)


def get_lunar_month11(yy):
    off = jd_from_date(31, 12, yy) - 2415021
    k = INT(off / 29.530588853)
    nm = get_new_moon_day(k)
    if get_sun_longitude(nm) >= 9:
        nm = get_new_moon_day(k - 1)
    return nm


def get_leap_month_offset(a11):
    k = INT((a11 - 2415021.076998695) / 29.530588853 + 0.5)
    i = 1
    arc = get_sun_longitude(get_new_moon_day(k + i))
    while True:
        last = arc
        i += 1
        arc = get_sun_longitude(get_new_moon_day(k + i))
        if not (arc != last and i < 14):
            break
    return i - 1


def solar_to_lunar(jd):
    dd, mm, yy = date_from_jd(jd)
    k = INT((jd - 2415021.076998695) / 29.530588853)
    month_start = get_new_moon_day(k + 1)
    if month_start > jd:
        month_start = get_new_moon_day(k)
    a11 = get_lunar_month11(yy)
    b11 = a11
    if a11 >= month_start:
        lunar_year = yy
        a11 = get_lunar_month11(yy - 1)
    else:
        lunar_year = yy + 1
        b11 = get_lunar_month11(yy + 1)
    lunar_day = jd - month_start + 1
    diff = INT((month_start - a11) / 29)
    leap = False
    month = diff + 11
    if b11 - a11 > 365:
        leap_diff = get_leap_month_offset(a11)
        if diff >= leap_diff:
            month = diff + 10
            if diff == leap_diff:
                leap = True
    if month > 12:
        month -= 12
    if month >= 11 and diff < 4:
        lunar_year -= 1
    return lunar_day, month, leap, lunar_year, month_start


if __name__ == '__main__':
    base = jd_from_date(1, 1, 2026)
    end = jd_from_date(1, 1, 2041)
    k = INT((base - 2415021.076998695) / 29.530588853) - 1
    starts = []
    while True:
        d = get_new_moon_day(k)
        if d > end:
            starts.append(d)
            break
        starts.append(d)
        k += 1
    # tháng đầu: tháng chứa 1/1/2026
    months = []
    for i in range(len(starts) - 1):
        s, e = starts[i], starts[i + 1]
        if e <= base:
            continue
        day, month, leap, year, ms = solar_to_lunar(s)
        assert day == 1 and ms == s, (s, day)
        months.append({'s': s - base, 'd': e - s, 'm': month, 'l': 1 if leap else 0, 'y': year})
    out = sys.argv[1] if len(sys.argv) > 1 else None
    # Mỗi tháng một dòng cho dễ đọc / so sánh.
    text = json.dumps({'baseDate': '2026-01-01', 'timeZone': 7, 'months': months}, separators=(',', ':'))
    text = text.replace('},{', '},' + chr(10) + '{')
    if out:
        open(out, 'w', encoding='utf-8').write(text)
    print(len(months), 'months', len(text), 'bytes')
    for m in months:
        if m['m'] == 1 and not m['l']:
            print('Tet', m['y'], date_from_jd(base + m['s']))
        if m['l']:
            print('Nhuan', m['y'], 'thang', m['m'], date_from_jd(base + m['s']), m['d'])
