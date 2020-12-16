class JsonReflectorLib:
    def __init__(self) -> None:
        print("creating!")

    def test(self):
        pass

    def get_keyword_names(self):
        return ["foo", "bar"]

    def run_keyword(self, name, args, kwargs):
        print("running", name)        
