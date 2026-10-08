// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.tag.dto;

import java.beans.ConstructorProperties;

public class QuoteTagDto {
    public String tagType;
    public String tagTypeName;
    public Integer tagTypeLength;
    public Integer tagTypeStartIndex;
    public Integer tagTypeIndex;
    public String childTagType;
    public String childTag;
    public String childTagName;
    public String childTagValue;
    public String signField;
    public String pinyin;
    public String firstLetter;

    public static QuoteTagDtoBuilder builder() {
        return new QuoteTagDtoBuilder();
    }

    public String getTagType() {
        return this.tagType;
    }

    public String getTagTypeName() {
        return this.tagTypeName;
    }

    public Integer getTagTypeLength() {
        return this.tagTypeLength;
    }

    public Integer getTagTypeStartIndex() {
        return this.tagTypeStartIndex;
    }

    public Integer getTagTypeIndex() {
        return this.tagTypeIndex;
    }

    public String getChildTagType() {
        return this.childTagType;
    }

    public String getChildTag() {
        return this.childTag;
    }

    public String getChildTagName() {
        return this.childTagName;
    }

    public String getChildTagValue() {
        return this.childTagValue;
    }

    public String getSignField() {
        return this.signField;
    }

    public String getPinyin() {
        return this.pinyin;
    }

    public String getFirstLetter() {
        return this.firstLetter;
    }

    public void setTagType(String tagType) {
        this.tagType = tagType;
    }

    public void setTagTypeName(String tagTypeName) {
        this.tagTypeName = tagTypeName;
    }

    public void setTagTypeLength(Integer tagTypeLength) {
        this.tagTypeLength = tagTypeLength;
    }

    public void setTagTypeStartIndex(Integer tagTypeStartIndex) {
        this.tagTypeStartIndex = tagTypeStartIndex;
    }

    public void setTagTypeIndex(Integer tagTypeIndex) {
        this.tagTypeIndex = tagTypeIndex;
    }

    public void setChildTagType(String childTagType) {
        this.childTagType = childTagType;
    }

    public void setChildTag(String childTag) {
        this.childTag = childTag;
    }

    public void setChildTagName(String childTagName) {
        this.childTagName = childTagName;
    }

    public void setChildTagValue(String childTagValue) {
        this.childTagValue = childTagValue;
    }

    public void setSignField(String signField) {
        this.signField = signField;
    }

    public void setPinyin(String pinyin) {
        this.pinyin = pinyin;
    }

    public void setFirstLetter(String firstLetter) {
        this.firstLetter = firstLetter;
    }

    public boolean equals(Object o) {
        if (o == this) {
            return true;
        }
        if (!(o instanceof QuoteTagDto)) {
            return false;
        }
        QuoteTagDto other = (QuoteTagDto) o;
        if (!other.canEqual(this)) {
            return false;
        }
        String this$tagType = this.getTagType();
        String other$tagType = other.getTagType();
        if (this$tagType == null ? other$tagType != null : !this$tagType.equals(other$tagType)) {
            return false;
        }
        String this$tagTypeName = this.getTagTypeName();
        String other$tagTypeName = other.getTagTypeName();
        if (this$tagTypeName == null
                ? other$tagTypeName != null
                : !this$tagTypeName.equals(other$tagTypeName)) {
            return false;
        }
        Integer this$tagTypeLength = this.getTagTypeLength();
        Integer other$tagTypeLength = other.getTagTypeLength();
        if (this$tagTypeLength == null
                ? other$tagTypeLength != null
                : !((Object) this$tagTypeLength).equals(other$tagTypeLength)) {
            return false;
        }
        Integer this$tagTypeStartIndex = this.getTagTypeStartIndex();
        Integer other$tagTypeStartIndex = other.getTagTypeStartIndex();
        if (this$tagTypeStartIndex == null
                ? other$tagTypeStartIndex != null
                : !((Object) this$tagTypeStartIndex).equals(other$tagTypeStartIndex)) {
            return false;
        }
        Integer this$tagTypeIndex = this.getTagTypeIndex();
        Integer other$tagTypeIndex = other.getTagTypeIndex();
        if (this$tagTypeIndex == null
                ? other$tagTypeIndex != null
                : !((Object) this$tagTypeIndex).equals(other$tagTypeIndex)) {
            return false;
        }
        String this$childTagType = this.getChildTagType();
        String other$childTagType = other.getChildTagType();
        if (this$childTagType == null
                ? other$childTagType != null
                : !this$childTagType.equals(other$childTagType)) {
            return false;
        }
        String this$childTag = this.getChildTag();
        String other$childTag = other.getChildTag();
        if (this$childTag == null
                ? other$childTag != null
                : !this$childTag.equals(other$childTag)) {
            return false;
        }
        String this$childTagName = this.getChildTagName();
        String other$childTagName = other.getChildTagName();
        if (this$childTagName == null
                ? other$childTagName != null
                : !this$childTagName.equals(other$childTagName)) {
            return false;
        }
        String this$childTagValue = this.getChildTagValue();
        String other$childTagValue = other.getChildTagValue();
        if (this$childTagValue == null
                ? other$childTagValue != null
                : !this$childTagValue.equals(other$childTagValue)) {
            return false;
        }
        String this$signField = this.getSignField();
        String other$signField = other.getSignField();
        if (this$signField == null
                ? other$signField != null
                : !this$signField.equals(other$signField)) {
            return false;
        }
        String this$pinyin = this.getPinyin();
        String other$pinyin = other.getPinyin();
        if (this$pinyin == null ? other$pinyin != null : !this$pinyin.equals(other$pinyin)) {
            return false;
        }
        String this$firstLetter = this.getFirstLetter();
        String other$firstLetter = other.getFirstLetter();
        return !(this$firstLetter == null
                ? other$firstLetter != null
                : !this$firstLetter.equals(other$firstLetter));
    }

    protected boolean canEqual(Object other) {
        return other instanceof QuoteTagDto;
    }

    public int hashCode() {
        int PRIME = 59;
        int result = 1;
        String $tagType = this.getTagType();
        result = result * 59 + ($tagType == null ? 43 : $tagType.hashCode());
        String $tagTypeName = this.getTagTypeName();
        result = result * 59 + ($tagTypeName == null ? 43 : $tagTypeName.hashCode());
        Integer $tagTypeLength = this.getTagTypeLength();
        result = result * 59 + ($tagTypeLength == null ? 43 : ((Object) $tagTypeLength).hashCode());
        Integer $tagTypeStartIndex = this.getTagTypeStartIndex();
        result =
                result * 59
                        + ($tagTypeStartIndex == null
                                ? 43
                                : ((Object) $tagTypeStartIndex).hashCode());
        Integer $tagTypeIndex = this.getTagTypeIndex();
        result = result * 59 + ($tagTypeIndex == null ? 43 : ((Object) $tagTypeIndex).hashCode());
        String $childTagType = this.getChildTagType();
        result = result * 59 + ($childTagType == null ? 43 : $childTagType.hashCode());
        String $childTag = this.getChildTag();
        result = result * 59 + ($childTag == null ? 43 : $childTag.hashCode());
        String $childTagName = this.getChildTagName();
        result = result * 59 + ($childTagName == null ? 43 : $childTagName.hashCode());
        String $childTagValue = this.getChildTagValue();
        result = result * 59 + ($childTagValue == null ? 43 : $childTagValue.hashCode());
        String $signField = this.getSignField();
        result = result * 59 + ($signField == null ? 43 : $signField.hashCode());
        String $pinyin = this.getPinyin();
        result = result * 59 + ($pinyin == null ? 43 : $pinyin.hashCode());
        String $firstLetter = this.getFirstLetter();
        result = result * 59 + ($firstLetter == null ? 43 : $firstLetter.hashCode());
        return result;
    }

    public String toString() {
        return "QuoteTagDto(tagType="
                + this.getTagType()
                + ", tagTypeName="
                + this.getTagTypeName()
                + ", tagTypeLength="
                + this.getTagTypeLength()
                + ", tagTypeStartIndex="
                + this.getTagTypeStartIndex()
                + ", tagTypeIndex="
                + this.getTagTypeIndex()
                + ", childTagType="
                + this.getChildTagType()
                + ", childTag="
                + this.getChildTag()
                + ", childTagName="
                + this.getChildTagName()
                + ", childTagValue="
                + this.getChildTagValue()
                + ", signField="
                + this.getSignField()
                + ", pinyin="
                + this.getPinyin()
                + ", firstLetter="
                + this.getFirstLetter()
                + ")";
    }

    public QuoteTagDto() {}

    @ConstructorProperties(
            value = {
                "tagType",
                "tagTypeName",
                "tagTypeLength",
                "tagTypeStartIndex",
                "tagTypeIndex",
                "childTagType",
                "childTag",
                "childTagName",
                "childTagValue",
                "signField",
                "pinyin",
                "firstLetter"
            })
    public QuoteTagDto(
            String tagType,
            String tagTypeName,
            Integer tagTypeLength,
            Integer tagTypeStartIndex,
            Integer tagTypeIndex,
            String childTagType,
            String childTag,
            String childTagName,
            String childTagValue,
            String signField,
            String pinyin,
            String firstLetter) {
        this.tagType = tagType;
        this.tagTypeName = tagTypeName;
        this.tagTypeLength = tagTypeLength;
        this.tagTypeStartIndex = tagTypeStartIndex;
        this.tagTypeIndex = tagTypeIndex;
        this.childTagType = childTagType;
        this.childTag = childTag;
        this.childTagName = childTagName;
        this.childTagValue = childTagValue;
        this.signField = signField;
        this.pinyin = pinyin;
        this.firstLetter = firstLetter;
    }

    public static class QuoteTagDtoBuilder {
        private String tagType;
        private String tagTypeName;
        private Integer tagTypeLength;
        private Integer tagTypeStartIndex;
        private Integer tagTypeIndex;
        private String childTagType;
        private String childTag;
        private String childTagName;
        private String childTagValue;
        private String signField;
        private String pinyin;
        private String firstLetter;

        QuoteTagDtoBuilder() {}

        public QuoteTagDtoBuilder tagType(String tagType) {
            this.tagType = tagType;
            return this;
        }

        public QuoteTagDtoBuilder tagTypeName(String tagTypeName) {
            this.tagTypeName = tagTypeName;
            return this;
        }

        public QuoteTagDtoBuilder tagTypeLength(Integer tagTypeLength) {
            this.tagTypeLength = tagTypeLength;
            return this;
        }

        public QuoteTagDtoBuilder tagTypeStartIndex(Integer tagTypeStartIndex) {
            this.tagTypeStartIndex = tagTypeStartIndex;
            return this;
        }

        public QuoteTagDtoBuilder tagTypeIndex(Integer tagTypeIndex) {
            this.tagTypeIndex = tagTypeIndex;
            return this;
        }

        public QuoteTagDtoBuilder childTagType(String childTagType) {
            this.childTagType = childTagType;
            return this;
        }

        public QuoteTagDtoBuilder childTag(String childTag) {
            this.childTag = childTag;
            return this;
        }

        public QuoteTagDtoBuilder childTagName(String childTagName) {
            this.childTagName = childTagName;
            return this;
        }

        public QuoteTagDtoBuilder childTagValue(String childTagValue) {
            this.childTagValue = childTagValue;
            return this;
        }

        public QuoteTagDtoBuilder signField(String signField) {
            this.signField = signField;
            return this;
        }

        public QuoteTagDtoBuilder pinyin(String pinyin) {
            this.pinyin = pinyin;
            return this;
        }

        public QuoteTagDtoBuilder firstLetter(String firstLetter) {
            this.firstLetter = firstLetter;
            return this;
        }

        public QuoteTagDto build() {
            return new QuoteTagDto(
                    this.tagType,
                    this.tagTypeName,
                    this.tagTypeLength,
                    this.tagTypeStartIndex,
                    this.tagTypeIndex,
                    this.childTagType,
                    this.childTag,
                    this.childTagName,
                    this.childTagValue,
                    this.signField,
                    this.pinyin,
                    this.firstLetter);
        }

        public String toString() {
            return "QuoteTagDto.QuoteTagDtoBuilder(tagType="
                    + this.tagType
                    + ", tagTypeName="
                    + this.tagTypeName
                    + ", tagTypeLength="
                    + this.tagTypeLength
                    + ", tagTypeStartIndex="
                    + this.tagTypeStartIndex
                    + ", tagTypeIndex="
                    + this.tagTypeIndex
                    + ", childTagType="
                    + this.childTagType
                    + ", childTag="
                    + this.childTag
                    + ", childTagName="
                    + this.childTagName
                    + ", childTagValue="
                    + this.childTagValue
                    + ", signField="
                    + this.signField
                    + ", pinyin="
                    + this.pinyin
                    + ", firstLetter="
                    + this.firstLetter
                    + ")";
        }
    }
}
