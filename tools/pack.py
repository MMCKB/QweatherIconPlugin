#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""把 Release 构建产物打包为 ClassIsland 插件包（.cipx）。

.cipx 本质上是一个 ZIP，manifest.yml 必须位于压缩包根目录。

由于 ClassIsland 2.1.1 起迁移到 .NET 10 + Avalonia 12，与 2.0.x~2.1.0（.NET 8 + Avalonia 11）
二进制不兼容，这里按目标框架分别打包：

    net10.0 -> manifest.yml        apiVersion 2.1.1.0   （ClassIsland 2.1.1+）
    net8.0  -> manifest.legacy.yml apiVersion 2.0.0.0   （ClassIsland 2.0.0 ~ 2.1.0）

用法：
    dotnet build -c Release
    python tools/pack.py                  # 两个都打
    python tools/pack.py --only net8.0    # 只打 net8（CI 用）
    python tools/pack.py --out ./dist     # 指定输出目录
"""

from __future__ import annotations

import argparse
import re
import sys
import zipfile
from pathlib import Path

# Windows 上 stdout 的默认编码不一定是 UTF-8（GitHub Actions 的 windows runner 上就不是），
# 打印中文会直接抛 UnicodeEncodeError。这里强制切成 UTF-8。
for _stream in (sys.stdout, sys.stderr):
    try:
        _stream.reconfigure(encoding="utf-8")
    except (AttributeError, ValueError):
        pass

ROOT = Path(__file__).resolve().parent.parent
BUILD_ROOT = ROOT / "bin" / "Release"
DEFAULT_OUT = ROOT / "cipx"

# 目标框架 -> (输出后缀, 使用的清单文件)
TARGETS = {
    "net10.0": ("net10", "manifest.yml"),
    "net8.0": ("net8", "manifest.legacy.yml"),
}

# 打进插件包的文件（白名单，避免把 pdb、xml 等无关文件带进去）
INCLUDE_SUFFIXES = (".dll", ".json", ".yml", ".png", ".md")
EXCLUDE_SUFFIXES = (".pdb", ".xml")

# 无扩展名但必须随包分发（CC BY 4.0 要求署名信息随素材一同分发）
INCLUDE_NAMES = ("LICENSE", "LICENSE.txt", "THIRD-PARTY-NOTICES.md")


def read_version(manifest: Path) -> str:
    text = manifest.read_text(encoding="utf-8")
    match = re.search(r"^version:\s*(\S+)\s*$", text, re.MULTILINE)
    if not match:
        raise SystemExit(f"{manifest.name} 中找不到 version 字段")
    return match.group(1)


def pack(tfm: str, suffix: str, manifest_name: str, out_dir: Path) -> Path | None:
    build_dir = BUILD_ROOT / tfm
    manifest = ROOT / manifest_name

    if not build_dir.is_dir():
        print(f"跳过 {tfm}：找不到构建输出 {build_dir}")
        return None
    if not manifest.is_file():
        print(f"跳过 {tfm}：找不到清单 {manifest}")
        return None

    version = read_version(manifest)
    out_file = out_dir / f"QWeatherIconsPlugin-{version}-{suffix}.cipx"
    if out_file.exists():
        out_file.unlink()

    files = sorted(
        p for p in build_dir.iterdir()
        if p.is_file()
        and (p.name in INCLUDE_NAMES or p.suffix.lower() in INCLUDE_SUFFIXES)
        and p.suffix.lower() not in EXCLUDE_SUFFIXES
        and not p.name.startswith("manifest.")
    )
    # 用目标清单覆盖构建目录里的 manifest.yml
    files = [p for p in files if p.name != "manifest.yml"]

    present = {p.name for p in files}
    missing = {"QWeatherIconsPlugin.dll"} - present
    if missing:
        print(f"{tfm} 构建产物缺少必需文件：{sorted(missing)}", file=sys.stderr)
        return None

    with zipfile.ZipFile(out_file, "w", zipfile.ZIP_DEFLATED) as zf:
        for path in files:
            zf.write(path, path.name)
        zf.write(manifest, "manifest.yml")

    size_kb = out_file.stat().st_size / 1024
    print(f"已打包 {out_file.name}  （{tfm}，清单 {manifest_name}）")
    print(f"  {len(files) + 1} 个文件，{size_kb:.1f} KB")
    for path in files:
        print(f"    {path.name}")
    print("    manifest.yml")
    return out_file


def main() -> int:
    parser = argparse.ArgumentParser(description="打包 ClassIsland 插件包 (.cipx)")
    parser.add_argument("--only", choices=sorted(TARGETS),
                        help="只打包指定的目标框架（CI 用 --only net8.0）")
    parser.add_argument("--out", type=Path, default=DEFAULT_OUT,
                        help=f"输出目录，默认为 {DEFAULT_OUT}")
    args = parser.parse_args()

    out_dir = args.out
    out_dir.mkdir(parents=True, exist_ok=True)

    targets = {args.only: TARGETS[args.only]} if args.only else TARGETS
    results = [pack(tfm, suffix, manifest, out_dir)
               for tfm, (suffix, manifest) in targets.items()]

    if not any(results):
        print("没有打包任何插件包", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
