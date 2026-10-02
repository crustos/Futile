#!/usr/bin/env python3
"""Prove the Unity build is untouched: preprocess each changed .cs with CRUST
undefined and compare to the file at the base commit. Handles #if [!]CRUST,
#else, #endif (nested, and other #if conditions are passed through as text)."""
import subprocess, sys, re
REPO = '/home/claude/Futile'
BASE = sys.argv[1] if len(sys.argv) > 1 else 'master'

def strip(text, crust=False):
    out, stack = [], []   # stack of (active_before, cond_value, in_else, is_crust_if)
    active = True
    for line in text.split('\n'):
        t = line.strip()
        m = re.match(r'#if\s+(!?)CRUST\s*$', t)
        if m:
            val = crust if not m.group(1) else (not crust)
            stack.append((active, val, False, True)); active = active and val; continue
        if t.startswith('#if'):
            stack.append((active, True, False, False))
            if active: out.append(line)
            continue
        if t == '#else' and stack and stack[-1][3]:
            a, v, _, c = stack[-1]; stack[-1] = (a, v, True, c); active = a and (not v); continue
        if t == '#endif' and stack:
            a, v, e, c = stack.pop(); active = a
            if not c and active: out.append(line)
            continue
        if active: out.append(line)
    assert not stack, 'unbalanced #if'
    return '\n'.join(out)

changed = subprocess.check_output(['git','-C',REPO,'diff','--name-only',BASE,'--','*.cs']).decode().split()
bad = 0
for f in changed:
    cur = open(f'{REPO}/{f}').read()
    try: base = subprocess.check_output(['git','-C',REPO,'show',f'{BASE}:{f}']).decode()
    except subprocess.CalledProcessError: print('NEW  ', f); continue
    try: ok = strip(cur) == base
    except AssertionError as e: print('ERR  ', f, e); bad += 1; continue
    print('same ' if ok else 'DIFF ', f); bad += (not ok)
sys.exit(1 if bad else 0)
