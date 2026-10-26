#!/usr/bin/env python3
"""Read-only verification of the local source snapshot used by this reference.

Standard library only. Does not launch Unity, install packages, or write files.
Exit 0: tracked evidence unchanged. Exit 1: changed/missing evidence. Exit 2: usage/data error.
This path-based snapshot does not detect newly added files or untracked art changes.
"""
import argparse
import hashlib
import json
from pathlib import Path


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--project', required=True, type=Path, help='Unity project root containing Assets, Packages and ProjectSettings')
    parser.add_argument('--json', action='store_true', help='Print machine-readable results')
    args = parser.parse_args()
    root = args.project.expanduser().resolve()
    if not (root/'Assets').is_dir() or not (root/'ProjectSettings').is_dir():
        parser.error('The selected directory does not look like a Unity project root.')
    source = Path(__file__).resolve().parent/'source_fingerprints.json'
    try:
        entries = json.loads(source.read_text(encoding='utf-8'))['files']
    except (OSError, ValueError, KeyError) as error:
        parser.error('Cannot read snapshot: '+str(error))
    changed, missing = [], []
    for entry in entries:
        rel = Path(entry['path'])
        if rel.is_absolute() or '..' in rel.parts:
            parser.error('Snapshot contains an unsafe relative path.')
        path = root/rel
        try:
            actual = hashlib.sha256(path.read_bytes()).hexdigest()
        except FileNotFoundError:
            missing.append(entry['path'])
            continue
        except OSError as error:
            parser.error('Cannot read '+str(path)+': '+str(error))
        if actual != entry['sha256']:
            changed.append(entry['path'])
    result = dict(status='unchanged' if not changed and not missing else 'review_changed_evidence',
                  tracked_files=len(entries), changed=changed, missing=missing,
                  limit='Does not detect added files, all art/scene changes or Unity runtime compatibility.')
    if args.json:
        print(json.dumps(result, indent=2))
    else:
        print('Reference snapshot: '+result['status'])
        print('Tracked files checked: '+str(len(entries)))
        for kind, paths in [('CHANGED',changed),('MISSING',missing)]:
            for path in paths:
                print(kind+': '+path)
        print(result['limit'])
    return 1 if changed or missing else 0


if __name__ == '__main__':
    raise SystemExit(main())
