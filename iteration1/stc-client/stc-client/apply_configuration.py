import os
import os.path
import yaml
from optparse import OptionParser
from jinja2 import Environment, FileSystemLoader, select_autoescape, Template


def render_templates(templates_folder, target_folder, context):
  env = Environment(
      loader=FileSystemLoader(templates_folder),
      autoescape=select_autoescape(['html', 'xml']))

  for name in env.list_templates():
    template = env.get_template(name)
    output = os.path.join(target_folder, template.name)
    os.makedirs(os.path.dirname(output), exist_ok=True)
    template.stream(context).dump(output)


def import_context(yamlfile, root_name, env):
  with open(yamlfile) as fp:
    data = yaml.load(fp)
  config = data.get(root_name)
  context = config.get('common', {})
  context.update(config.get('envs', {}).get(env))
  return context


def main():
  parser = OptionParser(
      usage="usage: %prog -i <templates_dir> -o <outputs_dir> -c <context_file> -n <root_name> -e <env>"
  )
  parser.add_option(
      "-i", help="The templates dir.", action="store", type="string")
  parser.add_option(
      "-o",
      help="The output dir for rendered templates.",
      action="store",
      type="string")
  parser.add_option(
      "-c",
      help="The context file, it should be a yaml file.",
      action="store",
      type="string")
  parser.add_option(
      "-n",
      help="The root name of the yaml file.",
      action="store",
      type="string")
  parser.add_option(
      "-e", help="Which env you want to apply.", action="store", type="string")
  (options, args) = parser.parse_args()

  context = import_context(options.c, options.n, options.e)
  render_templates(
      templates_folder=options.i, target_folder=options.o, context=context)


if __name__ == "__main__":
  main()