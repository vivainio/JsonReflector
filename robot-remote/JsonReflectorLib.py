import jsonreflector_client

keyword_dict = None


def get_keywords():
    global keyword_dict

    if keyword_dict:
        return dict
    kws = jsonreflector_client.get_keyword_names()
    keyword_dict = {}
    for kw in kws:
        key = kw.klass + " " + kw.method
        keyword_dict[key] = kw

    return keyword_dict.keys()


class JsonReflectorLib:
    def get_keyword_names(self):
        return get_keywords()

    def run_keyword(self, name, args, kwargs):
        kw = keyword_dict[name]
        return jsonreflector_client.run_keyword(kw, args)
