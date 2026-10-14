#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""生成插件图标 icon.png —— 16×16 点阵像素风格。

做法：先用字符网格描述 16×16 的点阵（每个字符 = 一个像素单元），
再以「最近邻」放大到 256×256，得到边界锐利的像素块效果（不做插值）。

用法：
    python tools/gen_icon.py            # 生成图标
    python tools/gen_icon.py --dump     # 只打印点阵，便于校对
"""

from __future__ import annotations

import sys
from pathlib import Path

from PIL import Image

# Windows 上 stdout 的默认编码不一定是 UTF-8（GitHub Actions 的 windows runner 上就不是），
# 打印中文会直接抛 UnicodeEncodeError。这里强制切成 UTF-8。
for _stream in (sys.stdout, sys.stderr):
    try:
        _stream.reconfigure(encoding="utf-8")
    except (AttributeError, ValueError):
        pass

ROOT = Path(__file__).resolve().parent.parent
OUT = ROOT / "icon.png"

GRID = 16          # 点阵分辨率（16×16）
SCALE = 16         # 放大倍数 → 256×256（整数倍，保证像素块不失真）

# 调色板：每个字符 = 一种颜色
PALETTE = {
    ".": None,             # 透明
    "b": "#2F6FBF",        # 背景蓝
    "Y": "#FFCE54",        # 太阳（亮）
    "O": "#F2A93B",        # 太阳（暗部）
    "W": "#FFFFFF",        # 云（亮）
    "c": "#CFDCE8",        # 云（暗部）
}

# 16×16 点阵：
#   外框为圆角方形背景；右上为太阳（直径 6 的圆 + 右下暗部）；下方为云（顶部有凸起）。
GRID_MAP = [
    "..bbbbbbbbbbbb..",
    ".bbbbbbbbbYYbbb.",
    "bbbbbbbbbYYYYbbb",
    "bbbbbbbbYYYYYYbb",
    "bbbbbbbbYYYYYObb",
    "bbbbbbbbbYYOObbb",
    "bbbbbbbbbbOObbbb",
    "bbbbbbWWWWWbbbbb",  # 云顶凸起
    "bbbWWWWWWWWbbbbb",   # 加宽（宽 8）
    "bbWWWWWWWWWWWbbb",  # 平底
    "bbWWWWWWWWWWWbbb",
    "bbWWWWWWWWWWWbbb",
    "bbbcccccccccbbbb",  # 底部阴影
    "bbbbbbbbbbbbbbbb",
    ".bbbbbbbbbbbbbb.",
    "..bbbbbbbbbbbb..",
]


def validate(grid: list[str]) -> None:
    if len(grid) != GRID:
        raise SystemExit(f"点阵应有 {GRID} 行，实际 {len(grid)} 行")
    for y, row in enumerate(grid):
        if len(row) != GRID:
            raise SystemExit(f"第 {y} 行应有 {GRID} 列，实际 {len(row)} 列：{row!r}")
        unknown = set(row) - set(PALETTE)
        if unknown:
            raise SystemExit(f"第 {y} 行含未定义字符：{unknown}")


def render(grid: list[str]) -> Image.Image:
    """按点阵逐格填充颜色，得到 16×16 原图。"""
    img = Image.new("RGBA", (GRID, GRID), (0, 0, 0, 0))
    for y, row in enumerate(grid):
        for x, ch in enumerate(row):
            color = PALETTE[ch]
            if color is None:
                continue
            r, g, b = (int(color[i:i + 2], 16) for i in (1, 3, 5))
            img.putpixel((x, y), (r, g, b, 255))
    # 最近邻放大：保持像素块锐利
    return img.resize((GRID * SCALE, GRID * SCALE), Image.NEAREST)


def main() -> int:
    validate(GRID_MAP)

    if "--dump" in sys.argv:
        print("   " + "".join(f"{i % 10}" for i in range(GRID)))
        for y, row in enumerate(GRID_MAP):
            print(f"{y:2d} " + " ".join(row))
        return 0

    img = render(GRID_MAP)
    img.save(OUT, "PNG")
    print(f"已生成 {OUT}  {img.size[0]}×{img.size[1]}（{GRID}×{GRID} 点阵 ×{SCALE}）")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
