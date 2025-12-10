# RAG-Code-Base Documentation

## Overview
This project implements a unified parser framework using TreeSitter.DotNet.  
It supports multiple programming languages and text formats.

## Features
- Extracts classes, functions, enums, and interfaces from source code.
- Handles content blocks in HTML, CSS, and Markdown.
- Generates InfoBlocks with metadata for each parsed element.

## Usage
1. Place source files in the `Data` directory.
2. Run the parser service.
3. Check the output InfoBlocks for analysis or embedding.

## Notes
- Ensure TreeSitter grammars are available for each language.
- Update the parser mappings if new languages are added.
