export function extractBetweenMarkers(fileContent: string): string {
    const startMarker = "// <FUMADOCS BEGIN>";
    const endMarker = "// <FUMADOCS END>";
    const startIndex = fileContent.indexOf(startMarker);
    const endIndex = fileContent.indexOf(endMarker, startIndex + startMarker.length);
    return startIndex === -1 || endIndex === -1
        ? fileContent
        : fileContent.substring(startIndex + startMarker.length, endIndex).trim();
}

// Takes a string of the following formats
// %d - number
// %d..%d number from-to range
// returns an array of the numbers in the range, or the specific number
// throws error if %d cannot be parsed as an int, or if the range is not an ascending range
function _createRangeCode(range: string) {
    const ranges = [];
    const split = range.split("..")
    if (split.length === 1) {
        // single number mode
        ranges.push(parseInt(range))
    }
    else if (split.length === 2) {
        // range mode
        const a = parseInt(split[0]);
        const b = parseInt(split[1]);
        const len = b - a + 1;
        if (len < 0) {
            throw `Invalid range: {range}`;
        }
        else if (len === 0) {
            ranges.push(a);
        }
        else {
            return [...Array(len).keys()].map(x => x + a);
        }
    }
    else {
        throw `Invalid range: {range}`;
    }
    return ranges;
}

export function createRanges(ranges: string) {
    try {
        return ranges.split(",").flatMap(x => _createRangeCode(x))
    }
    catch (ex) {
        throw `Error parsing: ${ranges} : ${ex}`
    }
}