# Gameboy (DMG) emulator
This video [You Should Make a GameBoy Emulator](https://www.youtube.com/watch?v=hy2yY5a1Z-0) made a lot of good points and helped motivate me to start this project.

The clear goal (through community-tests and gameboy roms) and documentation, the fact that it seemed just outside my comfort-zone, made the project not seem insurmountable, but still engaging enough to keep me motivated.

It also seemed like a great way to more closely familiarize myself with the basic theory behind the technology I use daily, without having to dive into straight hardware. 

## Things I would do differently
* CPU
    * Group the instructions by type instead of ordering them by Opcode.\
    Doing it this way was error prone and resulted in a lot of jumping around when implementing them.\
    It also resulted in alot of missed opportunities for code deduplication on the initial implementation that was then later changed.

## Links to resources used
Most things related to gameboy emulator development can be found at https://gbdev.io/, but the following links are pages that I frequented during my development. 
### Pandocs
* https://gbdev.io/pandocs

### ROMs
* https://gbdev.gg8.se/files/roms/bootroms/
* https://gbdev.gg8.se/files/roms/blargg-gb-tests/

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