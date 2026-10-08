# Third-party notices

BTVReplay is released under the GNU General Public License v3.0 (`LICENSE`). This file lists the code,
binaries and text from other projects that the repository contains or that the built application
ships, with each project's licence. Every source file adapted from one of them carries a credit line
pointing to this file; an edit-mode test (`ThirdPartyNoticesTests`) checks that the files listed here
exist and carry it.

| Component | Licence | Shipped as |
|---|---|---|
| HiBoP | BSD-3-Clause | adapted C# source |
| Unity UI Extensions | BSD-3-Clause | adapted C# source |
| UnityStandaloneFileBrowser | MIT | C# source, macOS and Linux natives |
| Ookii.Dialogs | BSD-3-Clause | Windows DLL |
| Mono `System.Windows.Forms` | MIT | Windows DLL |
| Json.NET | MIT | managed DLL |
| SimpleExpressionEngine | CC0-1.0 | C# source |
| FFTW 3 | GPL-2.0-or-later | linked into the Framework plugin |
| LLVM OpenMP runtime | Apache-2.0 WITH LLVM-exception | linked into the macOS Framework plugin |
| GCC `libgomp` | GPL-3.0 WITH GCC-exception-3.1 | Linux plugin |
| Microsoft Visual C++ runtime | Microsoft distributable code | Windows DLLs |
| pstack | MIT | adapted text in a Claude Code skill |

## HiBoP

- Source: https://github.com/hbp-HiBoP/HiBoP (CRNL, Human Brain Project)
- Licence: BSD-3-Clause
- Adapted in (list, tooltip, menu and resizable-grid tools):
  - `Assets/Scripts/Tools/CustomList/ActionableList.cs`
  - `Assets/Scripts/Tools/CustomList/ActionnableItem.cs`
  - `Assets/Scripts/Tools/CustomList/BaseList.cs`
  - `Assets/Scripts/Tools/CustomList/GenericEvents.cs`
  - `Assets/Scripts/Tools/CustomList/IListable.cs`
  - `Assets/Scripts/Tools/CustomList/ISelectionCountable.cs`
  - `Assets/Scripts/Tools/CustomList/Item.cs`
  - `Assets/Scripts/Tools/CustomList/List.cs`
  - `Assets/Scripts/Tools/CustomList/SelectableItem.cs`
  - `Assets/Scripts/Tools/CustomList/SelectableList.cs`
  - `Assets/Scripts/Tools/Tooltip/Tooltip.cs`
  - `Assets/Scripts/Tools/Tooltip/TooltipManager.cs`
  - `Assets/Scripts/UI/Menu/Menu.cs`
  - `Assets/Scripts/UI/SceneUI/Columns/VerticalHandler.cs`

```
Copyright (c) 2026, Benjamin BONTEMPS, Adrien GANNERIE, Florian LANCE
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

3. Neither the name of the copyright holder nor the names of its
   contributors may be used to endorse or promote products derived from
   this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
```

## Unity UI Extensions

- Source: https://github.com/Unity-UI-Extensions/com.unity.uiextensions (reached BTVReplay through HiBoP)
- Licence: BSD-3-Clause
- Adapted in:
  - `Assets/Scripts/UI/Tools/RangeSlider.cs`
  - `Assets/Scripts/UI/Tools/Editor/RangeSliderEditor.cs`
  - `Assets/Scripts/UI/Tools/SetPropertyUtility.cs`
- `RangeSlider` is credited upstream to Ben MacKinnon (@Dover8). `SetPropertyUtility` is the helper
  Unity UI Extensions copied from Unity's own uGUI source, where it is internal.

```
Copyright (c) 2019

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

3. Neither the name of the copyright holder nor the names of its
   contributors may be used to endorse or promote products derived from
   this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
```

## UnityStandaloneFileBrowser

- Source: https://github.com/gkngkc/UnityStandaloneFileBrowser
- Licence: MIT
- Included in: `Assets/Tools/StandaloneFileBrowser/` (C# sources,
  `Plugins/StandaloneFileBrowser.bundle` for macOS, `Plugins/Linux/x86_64/libStandaloneFileBrowser.so`)

```
MIT License

Copyright (c) 2017 Gökhan Gökçe

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

## Ookii.Dialogs

- Source: https://github.com/ookii-dialogs/ookii-dialogs-winforms (the bundled 1.0.0.0 binary predates
  that repository and carries Sven Groot's notice)
- Licence: BSD-3-Clause
- Included in: `Assets/Tools/StandaloneFileBrowser/Plugins/Ookii.Dialogs.dll` (Windows file dialogs)

```
Copyright (c) Sven Groot (Ookii.org) 2009
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

3. Neither the name of the copyright holder nor the names of its
   contributors may be used to endorse or promote products derived from
   this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
```

## Mono System.Windows.Forms

- Source: https://github.com/mono/mono (`mcs/class/System.Windows.Forms`)
- Licence: MIT (X11)
- Included in: `Assets/Tools/StandaloneFileBrowser/Plugins/System.Windows.Forms.dll` (Windows file dialogs)

```
Copyright (c) Novell, Inc. (Mono Project)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

## Json.NET

- Source: https://github.com/JamesNK/Newtonsoft.Json (version 12.0.3)
- Licence: MIT
- Included in: `Assets/Plugins/Managed/Newtonsoft.Json.dll`

```
The MIT License (MIT)

Copyright (c) 2007 James Newton-King

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

## SimpleExpressionEngine

- Source: https://github.com/toptensoftware/SimpleExpressionEngine (Topten Software)
- Licence: CC0-1.0 (public domain dedication, https://creativecommons.org/publicdomain/zero/1.0/); no
  notice is required, it is credited here for provenance
- Included in: `Assets/Tools/SimpleExpressionEngine/` (montage expressions)

## FFTW 3

- Source: https://www.fftw.org/
- Licence: GPL-2.0-or-later. BTVReplay uses it under GPL v3, whose text is in `LICENSE`.
- Included in: the Framework plugin (`Assets/Plugins/*/Framework*`), linked statically

```
Copyright (c) 2003, 2007-14 Matteo Frigo
Copyright (c) 2003, 2007-14 Massachusetts Institute of Technology
```

## LLVM OpenMP runtime

- Source: https://github.com/llvm/llvm-project/tree/main/openmp
- Licence: Apache-2.0 WITH LLVM-exception, https://llvm.org/LICENSE.txt
- Included in: `Assets/Plugins/macOS-arm64/Framework.bundle`, linked statically. The LLVM exception
  covers runtime code embedded in a compiled binary.

## GCC libgomp

- Source: https://gcc.gnu.org/
- Licence: GPL-3.0 with the GCC Runtime Library Exception 3.1,
  https://www.gnu.org/licenses/gcc-exception-3.1.html (GPL v3 text in `LICENSE`)
- Included in: `Assets/Plugins/Linux-x86_64/libgomp.so`

## Microsoft Visual C++ runtime

- `msvcp140.dll`, `vcruntime140.dll`, `concrt140.dll`, `vccorlib140.dll`, `vcomp140.dll` and
  `ucrtbase.dll` in `Assets/Plugins/Windows-x86_64/`
- Microsoft redistributable code, shipped unmodified next to the plugins under the Visual Studio and
  Windows SDK licence terms. It is not open source and is not covered by BTVReplay's licence.

## pstack

- Source: https://github.com/cursor/plugins/tree/main/pstack (commit `ecc249f`)
- Licence: MIT
- Adapted in:
  - `.claude/skills/verify-btv/SKILL.md`

```
MIT License

Copyright (c) 2026 Lauren Tan

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

## Not third-party

`EEGFormat`, `Framework`, `BTVReplayLibraryC++` and `AudioFormat` in `Assets/Plugins/` are developed
in-house alongside BTVReplay.

## Not yet documented

The reference data in `Assets/Config/Data/` (MNI meshes and EEG positions, the MarsAtlas index,
Brodmann areas) and the sounds in `Assets/Config/Sounds/` have no recorded origin or licence yet.
