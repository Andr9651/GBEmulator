# Gameboy (DMG) emulator
This video [You Should Make a GameBoy Emulator](https://www.youtube.com/watch?v=hy2yY5a1Z-0) made a lot of good points and helped motivate me to start this project.

The clear goal (through community-tests and gameboy roms) and documentation, and the fact that it seemed just outside my comfort-zone, made the project not seem insurmountable, but still engaging enough to keep me motivated.

It also seemed like a great way to more closely familiarize myself with the basic theory behind the technology I use daily, without having to dive straight into hardware. 

## Current status
<details open>
<summary>Blarrgs Test ROMs</summary>

* CPU_instrs
    - [x] 01-special.gb
    - [ ] 02-interrupts.gb
    - [x] 03-op sp,hl.gb
    - [x] 04-op r,imm.gb
    - [x] 05-op rp.gb
    - [x] 06-ld r,r.gb
    - [ ] 07-jr,jp,call,ret,rst.gb
    - [x] 08-misc instrs.gb
    - [x] 09-op r,r.gb
    - [x] 10-bit ops.gb
    - [x] 11-op a,(hl).gb
</details>


## Mistakes and things I would do differently
* CPU
    * Group the instructions by type instead of ordering them by Opcode.\
    Doing it this way was error prone and resulted in a lot of jumping around when implementing them.\
    It also resulted in alot of missed opportunities for code deduplication on the initial implementation that was then later changed.
    * Using a separated table for ProgramCounter (PC) increments.\
    I saw someone use a table to handle the M-cycle increments and wondered why the same wasn't done for the PC as that seemed like a similar problem. I didn't immediatly find a reason why so I just did it and faked incrementing the PC during instructions that relied on it. Turns out that it messed with instructions that set the PC directly as they would always be off. I generally overcomplicated the PC increments and even conflated it with M-cycles at some point, which resulted in me incrementing the PC on every memory read/write. When in reality it should just be incremented when reading data with the PC.  

## Links to resources used
Most things related to gameboy emulator development can be found at https://gbdev.io/, but the following links are pages that I frequented during my development. 
### Pandocs
* https://gbdev.io/pandocs

### ROMs
* https://gbdev.gg8.se/files/roms/bootroms/
* https://gbdev.gg8.se/files/roms/blargg-gb-tests/

### Emulator
* https://bgb.bircd.org/
    * This emulator contains a debugger that I used to compare my emulator against, and was pretty instrumental.
* https://robertheaton.com/gameboy-doctor/
    * This is not an emulator per se but rather a tool to compare logs between your emulator and one that actully works. It dramatically decreased the amount of manual work required to find issues, and I  wholeheartedly recommend using it.

### Instruction codes and explanations
* https://rgbds.gbdev.io/docs/v0.9.1/gbz80.7
    * Generally good explanations for the individual instructions
* https://gbdev.io/gb-opcodes//optables/octal
    * The octal view can help visualize the groups of similar instructions
* https://meganesu.github.io/generate-gb-opcodes/
    * Made right clicks mark the instruction, to help keep track of what was implemented, with the following snippet.
    ```js
    document.body.insertAdjacentHTML("beforeend",
    "<style>
        ._1YF9SdaMLIbNysqTE4bJrc button.implemented {
            background-color: rgba(0, 0, 0, 0.30);
        }
    </style>");

    document.querySelectorAll("[type=button]").forEach(node => {
        node.addEventListener("contextmenu",(e) => {
            e.preventDefault();
            node.classList.toggle('implemented');
        });  
    });
    ```
* https://gekkio.fi/files/gb-docs/gbctr.pdf
    * This helped as a sanity check for some of the instructions that I couldn't wrap my head around.

### Other
* https://github.com/waf/CSharpRepl
    * Executing some quick C# snippets without having to setup another enviroment or changing program.cs saved me alot of time.