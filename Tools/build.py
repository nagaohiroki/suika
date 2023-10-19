import os
import platform
import glob
import shutil
import subprocess
import time


def build_unity(proj_path):
    unity_exe = find_unity_exe(proj_path)
    if not unity_exe:
        print("unity not found")
        return
    logfile = os.path.join(proj_path, "Logs", "build.log")
    if os.path.exists(logfile):
        os.remove(logfile)
    cmd = [
        unity_exe,
        "-quit",
        "-batchmode",
        "-projectPath",
        proj_path,
        "-executeMethod",
        "AppBuilder.Build",
        "-logFile",
        logfile,
    ]
    proc = subprocess.Popen(cmd)
    while not os.path.exists(logfile):
        continue
    with open(logfile, "r") as f:
        while proc.poll() is None:
            line = f.readline()
            if line:
                print(line, end="")
            else:
                time.sleep(5)


def find_unity_exe(proj_path):
    second = os.path.join("UnityHub", "secondaryInstallPath.json")
    default = os.path.join("Unity", "Hub", "Editor")
    program_files = os.environ.get("ProgramFiles")
    if not program_files:
        program_files = ""
    appdata = os.environ.get("APPDATA")
    if not appdata:
        appdata = ""
    platform_dirs = {
        "Windows": {
            "default": os.path.join(program_files, default),
            "second": os.path.join(appdata, second),
            "exe": os.path.join("Editor", "Unity.exe"),
        },
        "Darwin": {
            "default": os.path.join("/Applications", default),
            "second": os.path.join(
                os.path.expanduser("~"), "Library", "Application Support", second
            ),
            "exe": os.path.join("Unity.app", "Contents", "MacOS", "Unity"),
        },
    }
    platform_dir = platform_dirs.get(platform.system())
    if platform_dir is None:
        return
    editor_dir = platform_dir["default"]
    editor_second = platform_dir["second"]
    if os.path.exists(editor_second):
        with open(editor_second, "r") as f:
            manual_path = f.readline().strip('"')
            if os.path.exists(manual_path):
                editor_dir = manual_path
    version_txt = os.path.join(proj_path, "ProjectSettings", "ProjectVersion.txt")
    with open(version_txt, "r") as f:
        version = f.readline().split(":")[1].strip()
        return os.path.join(editor_dir, version, platform_dir["exe"])


def build_xcode(proj_path):
    xcodepath = os.path.join(proj_path, "Builds", "iOS*", "*", "*.xcodeproj")
    ios_dirs = glob.glob(xcodepath)
    for ios_dir in ios_dirs:
        base_dir = os.path.dirname(ios_dir)
        subprocess.run("xcodebuild", cwd=base_dir)
        create_ipa(base_dir)


def create_ipa(base_dir):
    payload = "Payload"
    rom_dir = os.path.dirname(base_dir)
    dst_dir = os.path.join(rom_dir, payload)
    ipa = dst_dir + ".ipa"
    if os.path.exists(dst_dir):
        shutil.rmtree(dst_dir)
    if os.path.exists(ipa):
        os.remove(ipa)
    src_dir = os.path.join(base_dir, "build", "Release-iphoneos")
    shutil.copytree(src_dir, dst_dir, ignore_dangling_symlinks=True)
    shutil.make_archive(dst_dir, "zip", root_dir=rom_dir, base_dir=payload)
    os.rename(dst_dir + ".zip", ipa)
    print("created :" + ipa)


def main():
    proj_path = os.path.join(os.path.dirname(__file__), "..", "UnityProject")
    build_unity(proj_path)
    build_xcode(proj_path)


if __name__ == "__main__":
    main()
