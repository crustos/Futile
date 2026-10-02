#!/usr/bin/env python3
"""Per-file csrust baseline for the Futile -> crust port.

Each non-Editor script is translated on its own (`csrust -D CRUST`). A file is
  OK    if it translates,
  EXCL  if nothing is left of it once `CRUST` is defined (a deliberate exclusion;
        not counted as progress),
  FAIL  otherwise, with csrust's first diagnostic.
Usage: crust_baseline.py [--crust DIR] [--src DIR]"""
import os, re, subprocess, sys, argparse, importlib.util
here = os.path.dirname(os.path.abspath(__file__))
spec = importlib.util.spec_from_file_location('uvc', os.path.join(here, 'unity_view_check.py'))
src_text = open(os.path.join(here, 'unity_view_check.py')).read().split('changed =')[0]
ns = {}; exec(src_text.replace("BASE = sys.argv[1] if len(sys.argv) > 1 else 'master'", "BASE='master'"), ns)
strip = ns['strip']

ap = argparse.ArgumentParser()
ap.add_argument('--crust', default=os.path.expanduser('~/crust'))
ap.add_argument('--src', default=os.path.join(here, '..', 'FutileProject', 'Assets', 'Futile'))
a = ap.parse_args(); SRC = os.path.abspath(a.src)

def only_boilerplate(text):
    t = re.sub(r'//[^\n]*', '', text); t = re.sub(r'/\*.*?\*/', '', t, flags=re.S)
    t = re.sub(r'^\s*using\s+[\w.= ]+;\s*$', '', t, flags=re.M)
    return not t.strip()

res = {'OK': [], 'EXCL': [], 'FAIL': []}
files = sorted(os.path.join(d, f) for d, _, fs in os.walk(SRC) if 'Editor' not in d.split(os.sep) for f in fs if f.endswith('.cs'))
for f in files:
    rel = os.path.relpath(f, SRC)
    if only_boilerplate(strip(open(f).read(), crust=True)):
        res['EXCL'].append((rel, '')); continue
    try:
        p = subprocess.run([sys.executable, 'tools/csrust.py', f, '-o', '/tmp/_b.c', '-D', 'CRUST'],
                           cwd=a.crust, capture_output=True, text=True, timeout=120)
        out = (p.stdout + p.stderr).strip()
    except subprocess.TimeoutExpired:
        res['FAIL'].append((rel, 'TIMEOUT (120s)')); continue
    if p.returncode == 0 and not out.startswith('csrust:'): res['OK'].append((rel, ''))
    else:
        first = next((l for l in out.splitlines() if l.startswith('csrust:')), out.splitlines()[-1] if out else '?')
        res['FAIL'].append((rel, first[:150]))
for k in ('OK', 'EXCL', 'FAIL'):
    for rel, why in res[k]: print('%-5s %s%s' % (k, rel, (' :: ' + why) if why else ''))
print('\nok=%d excluded=%d fail=%d (of %d)' % (len(res['OK']), len(res['EXCL']), len(res['FAIL']), len(files)))
