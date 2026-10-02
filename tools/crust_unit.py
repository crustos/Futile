#!/usr/bin/env python3
"""Translate Futile as ONE unit with csrust (--coost), and report the first blocker.

csrust stops at the first thing it cannot lower, so this is a snapshot of where
the port is, not a count. Files that are entirely `#if !CRUST` are left out.
Usage: crust_unit.py [--crust DIR] [--coost DIR] [--src DIR] [-o out.c]"""
import os, re, subprocess, sys, argparse
here = os.path.dirname(os.path.abspath(__file__))
ns = {}
exec(open(os.path.join(here, 'unity_view_check.py')).read().split('changed =')[0]
     .replace("BASE = sys.argv[1] if len(sys.argv) > 1 else 'master'", "BASE='master'"), ns)
strip = ns['strip']
ap = argparse.ArgumentParser()
ap.add_argument('--crust', default=os.path.expanduser('~/crust'))
ap.add_argument('--coost', default=os.path.expanduser('~/coost'))
ap.add_argument('--src', default=os.path.join(here, '..', 'FutileProject', 'Assets', 'Futile'))
ap.add_argument('-o', default='/tmp/futile_unit.c')
a = ap.parse_args()
src = os.path.abspath(a.src)

def boilerplate(text):
    t = re.sub(r'//[^\n]*', '', text); t = re.sub(r'/\*.*?\*/', '', t, flags=re.S)
    t = re.sub(r'^\s*using\s+[\w.= ]+;\s*$', '', t, flags=re.M)
    return not t.strip()

files, skipped = [], []
for d, _, fs in os.walk(src):
    if 'Editor' in d.split(os.sep): continue
    for f in sorted(fs):
        if not f.endswith('.cs'): continue
        p = os.path.join(d, f)
        (skipped if boilerplate(strip(open(p).read(), crust=True)) else files).append(p)
files.sort()
print('unit: %d files (%d fully excluded under CRUST)' % (len(files), len(skipped)))
r = subprocess.run([sys.executable, 'tools/csrust.py'] + files + ['-o', a.o, '--coost', a.coost, '-D', 'CRUST'],
                   cwd=a.crust, capture_output=True, text=True, timeout=600)
out = (r.stdout + r.stderr).strip()
print('TRANSLATES' if r.returncode == 0 else out[:600])
sys.exit(r.returncode)
