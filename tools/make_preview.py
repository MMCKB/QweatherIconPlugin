#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""生成带天气名称的图标预览图（交付用）。

数据来源：tests/IconRenderTest 渲染出的 overview.png
    - 第 0~3 行（每行 10 格，共 40 格）= 线框风格，顺序同 CodeOrder
    - 第 4~7 行 = 填充风格

用法：python tools/make_preview.py
"""

from __future__ import annotations

from pathlib import Path

from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parent.parent
OVERVIEW = ROOT / "tests" / "IconRenderTest" / "bin" / "Release" / "net10.0" / "overview.png"
OUT = ROOT.parent / "和风天气图标预览.png"

SRC_CELL = 64
COLS = 10
CODES = [
    "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
    "10", "11", "12", "13", "14", "15", "16", "17", "18", "19",
    "20", "21", "22", "23", "24", "25", "26", "27", "28", "29",
    "30", "31", "32", "33", "34", "35", "53", "301", "302", "99",
]
NAMES = {
    "0": "晴", "1": "多云", "2": "阴", "3": "阵雨", "4": "雷阵雨",
    "5": "雷阵雨伴冰雹", "6": "雨夹雪", "7": "小雨", "8": "中雨", "9": "大雨",
    "10": "暴雨", "11": "大暴雨", "12": "特大暴雨", "13": "阵雪", "14": "小雪",
    "15": "中雪", "16": "大雪", "17": "暴雪", "18": "雾", "19": "冻雨",
    "20": "沙尘暴", "21": "小雨-中雨", "22": "中雨-大雨", "23": "大雨-暴雨",
    "24": "暴雨-大暴雨", "25": "大暴雨-特大暴雨", "26": "小雪-中雪",
    "27": "中雪-大雪", "28": "大雪-暴雪", "29": "浮尘",
    "30": "扬沙", "31": "强沙尘暴", "32": "飑*", "33": "龙卷风*",
    "34": "弱高吹雪*", "35": "轻雾", "53": "霾", "301": "雨", "302": "雪", "99": "未知",
}

CELL_W, CELL_H = 96, 104
ICON = 52
PAD_TOP = 6
BG = (255, 255, 255)
INK = (28, 32, 38)
MUTED = (120, 128, 138)
ACCENT = (42, 96, 178)

FONT = "C:/Windows/Fonts/msyh.ttc"


def load(size: int) -> ImageFont.FreeTypeFont:
    try:
        return ImageFont.truetype(FONT, size)
    except OSError:
        return ImageFont.load_default()


def main() -> int:
    src = Image.open(OVERVIEW).convert("RGB")
    f_name = load(13)
    f_code = load(11)
    f_head = load(20)

    rows_per_pack = len(CODES) // COLS          # 4
    head_h = 46
    W = COLS * CELL_W
    H = head_h * 2 + rows_per_pack * CELL_H * 2

    canvas = Image.new("RGB", (W, H), BG)
    draw = ImageDraw.Draw(canvas)

    y = 0
    for pack_index, (pack_name, src_row0) in enumerate(
        (("和风天气 · 线框", 0), ("和风天气 · 填充", rows_per_pack))
    ):
        draw.rectangle((0, y, W, y + head_h), fill=ACCENT)
        draw.text((18, y + 12), pack_name, font=f_head, fill=(255, 255, 255))
        y += head_h

        for i, code in enumerate(CODES):
            col, row = i % COLS, i // COLS
            x = col * CELL_W

            # 从 overview.png 裁出该格并缩放
            cell = src.crop((
                col * SRC_CELL,
                (src_row0 + row) * SRC_CELL,
                (col + 1) * SRC_CELL,
                (src_row0 + row + 1) * SRC_CELL,
            )).resize((ICON, ICON), Image.LANCZOS)

            cx = x + (CELL_W - ICON) // 2
            cy = y + row * CELL_H + PAD_TOP
            canvas.paste(cell, (cx, cy))

            name = NAMES[code]
            tw = draw.textlength(name, font=f_name)
            draw.text((x + (CELL_W - tw) / 2, cy + ICON + 6), name, font=f_name, fill=INK)

            label = f"小米 {code}"
            tw2 = draw.textlength(label, font=f_code)
            draw.text((x + (CELL_W - tw2) / 2, cy + ICON + 24), label, font=f_code, fill=MUTED)

        y += rows_per_pack * CELL_H

    canvas.save(OUT, "PNG")
    print(f"已生成 {OUT}  {canvas.size[0]}×{canvas.size[1]}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
