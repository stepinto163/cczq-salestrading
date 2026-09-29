package cgws.ficc.parser.compat;

/** Minimal Chinese numeral conversion (三千 -> 3000). Only what the confirmed sample needs. */
final class ChineseNumbers {

    private ChineseNumbers() {
    }

    static int digitOf(char c) {
        switch (c) {
            case '零':
                return 0;
            case '一':
            case '壹':
                return 1;
            case '二':
            case '两':
            case '贰':
                return 2;
            case '三':
            case '叁':
                return 3;
            case '四':
            case '肆':
                return 4;
            case '五':
            case '伍':
                return 5;
            case '六':
            case '陆':
                return 6;
            case '七':
            case '柒':
                return 7;
            case '八':
            case '捌':
                return 8;
            case '九':
            case '玖':
                return 9;
            default:
                return -1;
        }
    }

    static long unitOf(char c) {
        switch (c) {
            case '十':
            case '拾':
                return 10;
            case '百':
            case '佰':
                return 100;
            case '千':
            case '仟':
                return 1000;
            case '万':
                return 10000;
            case '亿':
                return 100000000L;
            default:
                return 0;
        }
    }

    static long toLong(String text) {
        long total = 0;
        long section = 0;
        long current = 0;
        for (int i = 0; i < text.length(); i++) {
            char c = text.charAt(i);
            int digit = digitOf(c);
            if (digit >= 0) {
                current = digit;
                continue;
            }
            long unit = unitOf(c);
            if (unit == 0) {
                continue;
            }
            if (unit >= 10000) {
                section += (current == 0 ? 1 : current) * unit;
                total += section;
                section = 0;
            } else {
                section += (current == 0 ? 1 : current) * unit;
            }
            current = 0;
        }
        return total + section + current;
    }
}
